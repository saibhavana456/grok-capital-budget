using System.Security.Claims;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace IT_BUDGET_MONITORING_PORTAL.Components.Auth;

/// <summary>
/// Auth from HTTP cookie (set by GET /account/establish/{ticket}).
/// Waits for <see cref="AuthCircuitHandler"/> so HttpContext.User is read when the circuit opens.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly IAuthService _authService;
    private readonly ILogger<CustomAuthStateProvider> _logger;
    private readonly ClaimsPrincipal? _ctorUser;
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private AuthenticationState _state = Anonymous;
    private int _completed;

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
        // Stay on Authorizing until circuit handler hydrates cookie user (or timeout).
        await Task.WhenAny(_ready.Task, Task.Delay(5000));
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
            if (_authService.CurrentUser != null)
            {
                _state = new AuthenticationState(CreatePrincipal(_authService.CurrentUser));
                return;
            }

            var principal = httpUser?.Identity?.IsAuthenticated == true ? httpUser : _ctorUser;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                var pf = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrWhiteSpace(pf))
                {
                    var user = await _authService.GetLoggedInUserByPfAsync(pf);
                    if (user != null)
                    {
                        _authService.SetCurrentUser(user);
                        _state = new AuthenticationState(CreatePrincipal(user));
                        return;
                    }

                    _logger.LogWarning("Auth cookie PF {Pf} not found or inactive in APP_USER", pf);
                }
            }

            _state = Anonymous;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Auth circuit initialization failed");
            _state = Anonymous;
        }
        finally
        {
            _ready.TrySetResult();
            NotifyAuthenticationStateChanged(Task.FromResult(_state));
        }
    }

    public static ClaimsPrincipal CreatePrincipal(LoggedInUserDto user)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.PfNo),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.RoleCode),
            new Claim("DeptId", user.DeptId?.ToString() ?? ""),
            new Claim("Token", user.Token ?? "")
        }, authenticationType: "ITBudgetAuth");
        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateCookiePrincipal(LoggedInUserDto user)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.PfNo),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, user.RoleCode),
            new Claim("DeptId", user.DeptId?.ToString() ?? "")
        }, authenticationType: CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
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
            "Circuit opened. Authenticated={Auth}, PF={Pf}",
            httpUser?.Identity?.IsAuthenticated,
            httpUser?.FindFirstValue(ClaimTypes.NameIdentifier));

        if (_authStateProvider is CustomAuthStateProvider custom)
            await custom.InitializeFromCircuitAsync(httpUser);
    }
}
