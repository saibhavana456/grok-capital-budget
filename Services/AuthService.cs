using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using IT_BUDGET_MONITORING_PORTAL.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

/// <summary>
/// Login: PF + AD password + captcha → JWT → USER_TOKEN (Personal/SCV pattern).
/// Staff name from SQL Server STAFF_DETAILS when OrganisationsDb is configured.
/// Auth:BypassAd=true skips AD HTTP call for local laptop testing against seeded APP_USER.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IStaffLookupService _staffLookup;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthService> _logger;
    private string? _expectedCaptchaAnswer;
    private LoggedInUserDto? _currentUser;

    public AuthService(
        AppDbContext db,
        IStaffLookupService staffLookup,
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthService> logger)
    {
        _db = db;
        _staffLookup = staffLookup;
        _config = config;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public LoggedInUserDto? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public void SetCurrentUser(LoggedInUserDto? user) => _currentUser = user;

    public async Task<CaptchaQuestionDto> GetCaptchaAsync()
    {
        var questions = await _db.LoginCaptchaQuestions
            .AsNoTracking()
            .Where(q => q.IsActive == "Y")
            .ToListAsync();

        if (questions.Count == 0)
            throw new InvalidOperationException("No captcha questions configured in LOGIN_CAPTCHA_QUESTION.");

        var pick = questions[Random.Shared.Next(questions.Count)];
        _expectedCaptchaAnswer = pick.AnswerText;
        return new CaptchaQuestionDto
        {
            QuestionId = pick.QId,
            QuestionText = pick.QuestionText
        };
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request, string expectedCaptchaAnswer)
    {
        if (string.IsNullOrWhiteSpace(request.PfNumber) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.CaptchaAnswer))
        {
            return Fail("PF Number, Password and Captcha are required.");
        }

        var answerToCheck = string.IsNullOrWhiteSpace(expectedCaptchaAnswer)
            ? _expectedCaptchaAnswer
            : expectedCaptchaAnswer;

        if (string.IsNullOrWhiteSpace(answerToCheck) ||
            !string.Equals(request.CaptchaAnswer.Trim(), answerToCheck.Trim(), StringComparison.OrdinalIgnoreCase))
            return Fail("Invalid captcha answer.");

        var pf = request.PfNumber.Trim();
        var appUser = await _db.AppUsers
            .Include(u => u.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PfNo == pf && u.IsActive == "Y");

        if (appUser == null)
            return Fail("User is not registered in APP_USER for this application.");

        var bypassAd = _config.GetValue("Auth:BypassAd", false);
        var adOk = bypassAd || await ValidateAgainstAdAsync(pf, request.Password);
        if (!adOk)
            return Fail("AD authentication failed. Invalid PF or password.");

        // Single active session check (Personal USER_TOKEN pattern)
        var encryptedUserId = EncryptoData.EncryptString(pf);
        var existing = await _db.UserTokens.FirstOrDefaultAsync(t => t.UserId == encryptedUserId);
        if (existing != null && !string.IsNullOrEmpty(existing.LastToken))
        {
            // Allow re-login by clearing previous token (Blazor UX); Personal blocks — we clear for laptop UAT friendliness
            _db.UserTokens.Remove(existing);
            await _db.SaveChangesAsync();
        }

        var staff = await _staffLookup.LookupByPfAsync(pf);
        var displayName = staff?.EmpName ?? appUser.UserName ?? pf;
        var designation = staff?.Designation ?? appUser.Designation;

        var token = CreateJwt(pf, appUser.RoleCode, displayName);

        _db.UserTokens.Add(new UserToken
        {
            UserId = encryptedUserId,
            UserName = displayName,
            LastToken = token,
            HashToken = EncryptoData.Sha256Base64(token),
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        var user = new LoggedInUserDto
        {
            UserId = appUser.UserId,
            PfNo = appUser.PfNo,
            UserName = displayName,
            RoleCode = appUser.RoleCode,
            DeptId = appUser.DeptId,
            DeptName = appUser.Department?.DeptName,
            Designation = designation,
            HasRevenueDept = appUser.Department?.HasRevenue == "Y",
            Token = token
        };
        _currentUser = user;

        return new LoginResultDto
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = user
        };
    }

    public async Task LogoutAsync(string pfNo)
    {
        var encryptedUserId = EncryptoData.EncryptString(pfNo);
        var tokens = await _db.UserTokens.Where(t => t.UserId == encryptedUserId).ToListAsync();
        if (tokens.Count > 0)
        {
            _db.UserTokens.RemoveRange(tokens);
            await _db.SaveChangesAsync();
        }
        _currentUser = null;
    }

    private async Task<bool> ValidateAgainstAdAsync(string pf, string password)
    {
        try
        {
            var adUrl = _config["ApiKey:AD_API_URL"];
            var serviceName = _config["ApiKey:M_service_Name"];
            var servicePwd = _config["ApiKey:M_Service_Pwd"];
            if (string.IsNullOrWhiteSpace(adUrl))
            {
                _logger.LogWarning("AD_API_URL not configured.");
                return false;
            }

            var client = _httpClientFactory.CreateClient("AdApi");
            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{serviceName}:{servicePwd}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
            }

            var payload = JsonSerializer.Serialize(new { user_id = pf, password });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{adUrl.TrimEnd('/')}/validateDomainUser", content);
            if (!response.IsSuccessStatusCode)
                return false;

            var body = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("validation_status", out var status))
                return string.Equals(status.GetString(), "true", StringComparison.OrdinalIgnoreCase);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AD validateDomainUser failed");
            return false;
        }
    }

    private string CreateJwt(string pf, string role, string name)
    {
        var key = _config["Jwt:Key"] ?? "UnionBankITBudgetMonitoringPortal_ChangeMe_32chars!";
        var issuer = _config["Jwt:Issuer"] ?? "unionbankofindia";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, pf),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken(
            issuer,
            issuer,
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static LoginResultDto Fail(string message) =>
        new() { Success = false, Message = message };
}
