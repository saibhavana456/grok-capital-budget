using System.Security.Claims;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace IT_BUDGET_MONITORING_PORTAL.Components.Auth;

/// <summary>
/// Blazor Server auth via AuthenticationStateProvider + ProtectedSessionStorage.
/// Do NOT call HttpContext.SignInAsync here — interactive circuits cannot set
/// response headers ("Headers are read-only, response has already started").
/// Cookie scheme remains registered in Program.cs only for DefaultChallengeScheme.
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IAuthService _authService;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    public CustomAuthStateProvider(
        ProtectedSessionStorage sessionStorage,
        IAuthService authService,
        ILogger<CustomAuthStateProvider> logger)
    {
        _sessionStorage = sessionStorage;
        _authService = authService;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authService.CurrentUser != null)
            return new AuthenticationState(CreatePrincipal(_authService.CurrentUser));

        // After F5 / new circuit, CurrentUser is empty. Restore from browser sessionStorage.
        // First JS interop calls can throw until the circuit is fully connected — retry so
        // AuthorizeRouteView stays on Authorizing instead of bouncing to /login.
        for (var attempt = 1; attempt <= 8; attempt++)
        {
            try
            {
                var result = await _sessionStorage.GetAsync<LoggedInUserDto>(AppConstants.SessionUserKey);
                if (result.Success && result.Value != null && !string.IsNullOrWhiteSpace(result.Value.PfNo))
                {
                    _authService.SetCurrentUser(result.Value);
                    return new AuthenticationState(CreatePrincipal(result.Value));
                }

                // Storage readable but empty — truly logged out
                return Anonymous;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogDebug(ex, "Auth restore waiting for circuit (attempt {Attempt})", attempt);
                await Task.Delay(40 * attempt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Auth state restore failed");
                break;
            }
        }

        return Anonymous;
    }

    public async Task MarkUserAsAuthenticated(LoggedInUserDto user)
    {
        _authService.SetCurrentUser(user);
        await _sessionStorage.SetAsync(AppConstants.SessionUserKey, user);
        var principal = CreatePrincipal(user);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        var pf = _authService.CurrentUser?.PfNo;
        if (!string.IsNullOrEmpty(pf))
            await _authService.LogoutAsync(pf);

        _authService.SetCurrentUser(null);
        try
        {
            await _sessionStorage.DeleteAsync(AppConstants.SessionUserKey);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Session clear skipped");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private static ClaimsPrincipal CreatePrincipal(LoggedInUserDto user)
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
}
