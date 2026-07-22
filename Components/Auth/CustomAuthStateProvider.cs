using System.Security.Claims;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace IT_BUDGET_MONITORING_PORTAL.Components.Auth;

/// <summary>
/// Blazor auth state + ASP.NET cookie sign-in so DefaultChallengeScheme exists
/// (fixes: No authenticationScheme / DefaultChallengeScheme found).
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IAuthService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    public CustomAuthStateProvider(
        ProtectedSessionStorage sessionStorage,
        IAuthService authService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<CustomAuthStateProvider> logger)
    {
        _sessionStorage = sessionStorage;
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            if (_authService.CurrentUser != null)
                return new AuthenticationState(CreatePrincipal(_authService.CurrentUser));

            var httpUser = _httpContextAccessor.HttpContext?.User;
            if (httpUser?.Identity?.IsAuthenticated == true)
            {
                var fromCookie = UserFromPrincipal(httpUser);
                if (fromCookie != null)
                {
                    _authService.SetCurrentUser(fromCookie);
                    return new AuthenticationState(httpUser);
                }
            }

            var result = await _sessionStorage.GetAsync<LoggedInUserDto>(AppConstants.SessionUserKey);
            if (result.Success && result.Value != null)
            {
                _authService.SetCurrentUser(result.Value);
                return new AuthenticationState(CreatePrincipal(result.Value));
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Auth state restore skipped (prerender).");
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public async Task MarkUserAsAuthenticated(LoggedInUserDto user)
    {
        _authService.SetCurrentUser(user);
        await _sessionStorage.SetAsync(AppConstants.SessionUserKey, user);

        var principal = CreatePrincipal(user);
        var http = _httpContextAccessor.HttpContext;
        if (http != null)
        {
            await http.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
                });
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        var pf = _authService.CurrentUser?.PfNo;
        if (!string.IsNullOrEmpty(pf))
            await _authService.LogoutAsync(pf);

        _authService.SetCurrentUser(null);
        await _sessionStorage.DeleteAsync(AppConstants.SessionUserKey);

        var http = _httpContextAccessor.HttpContext;
        if (http != null)
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private static LoggedInUserDto? UserFromPrincipal(ClaimsPrincipal user)
    {
        var pf = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(pf)) return null;
        return new LoggedInUserDto
        {
            PfNo = pf,
            UserName = user.FindFirstValue(ClaimTypes.Name) ?? pf,
            RoleCode = user.FindFirstValue(ClaimTypes.Role) ?? "",
            DeptId = long.TryParse(user.FindFirstValue("DeptId"), out var d) ? d : null,
            Token = user.FindFirstValue("Token") ?? ""
        };
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
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
