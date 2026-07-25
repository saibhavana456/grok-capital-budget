using System.Security.Claims;
using IT_BUDGET_MONITORING_PORTAL.Components;
using IT_BUDGET_MONITORING_PORTAL.Components.Auth;
using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog — same pattern as Personal/SCV (file under Log/)
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "Log"));
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "IT_BUDGET_MONITORING_PORTAL")
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(builder.Environment.ContentRootPath, "Log", "ITBudget-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger, dispose: true);

try
{
Log.Information("Starting IT Budget Monitoring Portal");

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
    Log.Information("OracleDb loaded Env={Env} | {Conn}", builder.Environment.EnvironmentName, safe);
    if (safe.Contains("YOUR_SCHEMA_USER", StringComparison.OrdinalIgnoreCase) ||
        safe.Contains("YOUR_PASSWORD", StringComparison.OrdinalIgnoreCase) ||
        safe.Contains("REPLACE_WITH_YOUR_PASSWORD", StringComparison.OrdinalIgnoreCase))
    {
        Log.Warning("Placeholder Oracle password still present. Edit appsettings.Development.json.");
    }
}

builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseOracle(oracleConn));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// SQL Server STAFF_DETAILS optional — leave OrganisationsDb empty to skip

builder.Services.AddHttpClient("AdApi");
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LoginTicketStore>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffLookupService, StaffLookupService>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<ICapitalService, CapitalService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();

// Cookie auth — session cookie (Personal/SCV live pattern): F5 OK, browser close → login again
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/account/logout";
        options.AccessDeniedPath = "/login";
        // Same lifetime as Personal/SCV JWT (120 minutes); not sliding — fixed window
        options.ExpireTimeSpan = TimeSpan.FromMinutes(120);
        options.SlidingExpiration = false;
        options.Cookie.Name = "ITBudget.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CircuitHandler, AuthCircuitHandler>();

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

// Browser GET sets/clears the auth cookie (Blazor circuit cannot write Set-Cookie).
app.MapGet("/account/establish/{ticket}", async (
    string ticket,
    HttpContext http,
    LoginTicketStore tickets) =>
{
    if (!tickets.TryTake(ticket, out var user) || user == null)
    {
        Log.Warning("Login ticket invalid or expired. Ticket={Ticket}", ticket);
        return Results.Redirect("/login");
    }

    var principal = CustomAuthStateProvider.CreateCookiePrincipal(user);
    // Session cookie (IsPersistent=false) — matches Personal/SCV sessionStorage: refresh OK, close browser → login
    await http.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new Microsoft.AspNetCore.Authentication.AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = false,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(120)
        });

    Log.Information("Auth session cookie established for PF={Pf} Role={Role}", user.PfNo, user.RoleCode);
    var home = user.RoleCode switch
    {
        AppConstants.Roles.Admin => "/admin/masters",
        AppConstants.Roles.Checker => "/checker",
        _ => "/portal"
    };
    return Results.Redirect(home);
}).AllowAnonymous();

app.MapGet("/account/logout", async (HttpContext http, IAuthService auth) =>
{
    var pf = http.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!string.IsNullOrWhiteSpace(pf))
    {
        await auth.LogoutAsync(pf);
        Log.Information("Logout for PF={Pf}", pf);
    }

    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
