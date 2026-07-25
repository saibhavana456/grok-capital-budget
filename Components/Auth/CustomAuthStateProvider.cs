using System.Security.Claims;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace IT_BUDGET_MONITORING_PORTAL.Components.Auth;

/// <summary>
/// Auth from HTTP cookie (set by GET /account/establish/{ticket}).
/// Cookie must carry TokenHash matching USER_TOKEN.HASH_TOKEN — cookie alone is not enough.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    public const string TokenHashClaimType = "TokenHash";

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly IAuthService _authService;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private readonly ClaimsPrincipal? _ctorUser;
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private AuthenticationState _state = Anonymous;
    private int _completed;
    private readonly SemaphoreSlim _revalidateLock = new(1, 1);
    private DateTime _lastRevalidateUtc = DateTime.MinValue;

    public CustomAuthStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IAuthService authService,
        ILogger<CustomAuthStateProvider> logger)
    {
        _authService = authService;
        _logger = logger;
        _ctorUser = httpContextAccessor.HttpContext?.User;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        await Task.WhenAny(_ready.Task, Task.Delay(5000));
        await RevalidateIfNeededAsync();
        return _state;
    }

    public async Task InitializeFromCircuitAsync(ClaimsPrincipal? httpUser)
    {
        if (Interlocked.Exchange(ref _completed, 1) == 1)
        {
            _ready.TrySetResult();
            return;
        }

        try
        {
            var principal = httpUser?.Identity?.IsAuthenticated == true ? httpUser : _ctorUser;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                var pf = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var tokenHash = principal.FindFirstValue(TokenHashClaimType);
                if (!string.IsNullOrWhiteSpace(pf))
                {
                    var user = await _authService.ValidateSessionAsync(pf, tokenHash);
                    if (user != null)
                    {
                        _authService.SetCurrentUser(user);
                        _state = new AuthenticationState(CreatePrincipal(user));
                        _lastRevalidateUtc = DateTime.UtcNow;
                        return;
                    }

                    _logger.LogWarning(
                        "Auth cookie for PF {Pf} rejected — no matching USER_TOKEN (cleared or other browser took session)",
                        pf);
                }
            }

            _authService.SetCurrentUser(null);
            _state = Anonymous;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Auth circuit initialization failed");
            _authService.SetCurrentUser(null);
            _state = Anonymous;
        }
        finally
        {
            _ready.TrySetResult();
            NotifyAuthenticationStateChanged(Task.FromResult(_state));
        }
    }

    /// <summary>
    /// Re-check USER_TOKEN periodically so Browser A loses access when session is cleared
    /// or another browser completes login after clear — without requiring F5.
    /// </summary>
    private async Task RevalidateIfNeededAsync()
    {
        if (_state.User.Identity?.IsAuthenticated != true)
            return;

        // Avoid hammering Oracle on every cascading auth read
        if ((DateTime.UtcNow - _lastRevalidateUtc).TotalSeconds < 5)
            return;

        if (!await _revalidateLock.WaitAsync(0))
            return;

        try
        {
            if ((DateTime.UtcNow - _lastRevalidateUtc).TotalSeconds < 5)
                return;

            var pf = _state.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hash = _state.User.FindFirstValue(TokenHashClaimType)
                       ?? _authService.CurrentUser?.TokenHash;
            var user = await _authService.ValidateSessionAsync(pf ?? "", hash);
            _lastRevalidateUtc = DateTime.UtcNow;

            if (user != null)
            {
                _authService.SetCurrentUser(user);
                return;
            }

            _logger.LogInformation("Session invalidated for PF {Pf} during revalidation", pf);
            _authService.SetCurrentUser(null);
            _state = Anonymous;
            NotifyAuthenticationStateChanged(Task.FromResult(_state));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Session revalidation failed");
        }
        finally
        {
            _revalidateLock.Release();
        }
    }

    public static ClaimsPrincipal CreatePrincipal(LoggedInUserDto user)
    {
        var hash = ResolveTokenHash(user);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.PfNo),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.RoleCode),
            new Claim("DeptId", user.DeptId?.ToString() ?? ""),
            new Claim(TokenHashClaimType, hash)
        }, authenticationType: "ITBudgetAuth");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateCookiePrincipal(LoggedInUserDto user)
    {
        var hash = ResolveTokenHash(user);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.PfNo),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.RoleCode),
            new Claim("DeptId", user.DeptId?.ToString() ?? ""),
            new Claim(TokenHashClaimType, hash)
        }, authenticationType: CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    private static string ResolveTokenHash(LoggedInUserDto user)
    {
        if (!string.IsNullOrWhiteSpace(user.TokenHash))
            return user.TokenHash;
        if (!string.IsNullOrWhiteSpace(user.Token))
            return EncryptoData.Sha256Base64(user.Token);
        return string.Empty;
    }
}

/// <summary>Scoped per circuit — reads cookie user while HttpContext is still available.</summary>
public sealed class AuthCircuitHandler : CircuitHandler
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthCircuitHandler> _logger;

    public AuthCircuitHandler(
        AuthenticationStateProvider authStateProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthCircuitHandler> logger)
    {
        _authStateProvider = authStateProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        var httpUser = _httpContextAccessor.HttpContext?.User;
        _logger.LogDebug(
            "Circuit opened. Authenticated={Auth}, PF={Pf}, HasHash={HasHash}",
            httpUser?.Identity?.IsAuthenticated,
            httpUser?.FindFirstValue(ClaimTypes.NameIdentifier),
            !string.IsNullOrEmpty(httpUser?.FindFirstValue(CustomAuthStateProvider.TokenHashClaimType)));

        if (_authStateProvider is CustomAuthStateProvider custom)
            await custom.InitializeFromCircuitAsync(httpUser);
    }
}
