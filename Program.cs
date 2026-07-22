using IT_BUDGET_MONITORING_PORTAL.Components;
using IT_BUDGET_MONITORING_PORTAL.Components.Auth;
using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Oracle app DB (IT_BUDGET_MONITORING_LOCAL / your schema owner)
var oracleConn = builder.Configuration.GetConnectionString("OracleDb") ?? string.Empty;
if (oracleConn.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
    oracleConn = EncryptoData.DecryptAes(oracleConn["ENC:".Length..]);

builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseOracle(oracleConn));
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// SQL Server STAFF_DETAILS (optional — leave OrganisationsDb empty to skip)
// StaffLookupService creates its own context when connection string is present.

builder.Services.AddHttpClient("AdApi");
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffLookupService, StaffLookupService>();
builder.Services.AddScoped<IMasterService, MasterService>();
builder.Services.AddScoped<ICapitalService, CapitalService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
