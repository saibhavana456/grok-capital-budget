using System.Security.Claims;
using IT_BUDGET_MONITORING_PORTAL.Components;
using IT_BUDGET_MONITORING_PORTAL.Components.Auth;
using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Oracle app DB — same user as SQL Developer connection (e.g. IT_BUDGET_MONITORING_PORTAL)
var oracleConn = builder.Configuration.GetConnectionString("OracleDb") ?? string.Empty;
if (oracleConn.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
    oracleConn = EncryptoData.DecryptAes(oracleConn["ENC:".Length..]);

// Safe diagnostic (no password): shows which connect string VS actually loaded
{
    var safe = oracleConn;
    foreach (var key in new[] { "Password=", "Pwd=" })
    {
        var i = safe.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (i < 0) continue;
        var start = i + key.Length;
        var end = safe.IndexOf(';', start);
        if (end < 0) end = safe.Length;
        safe = safe[..start] + "***" + safe[end..];
    }
    Console.WriteLine($"[OracleDb loaded] Env={builder.Environment.EnvironmentName} | {safe}");
    if (safe.Contains("YOUR_SCHEMA_USER", StringComparison.OrdinalIgnoreCase) ||
        safe.Contains("YOUR_PASSWORD", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("[OracleDb WARNING] Placeholder User/Password still present. " +
                          "Edit appsettings.Development.json (Development overrides appsettings.json).");
    }
}

builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseOracle(oracleConn));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// SQL Server STAFF_DETAILS optional — leave OrganisationsDb empty to skip

builder.Services.AddHttpClient("AdApi");
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffLookupService, StaffLookupService>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<ICapitalService, CapitalService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();

// Cookie scheme required so [Authorize] / AuthorizeRouteView have a DefaultChallengeScheme
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.Name = "ITBudget.Auth";
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
