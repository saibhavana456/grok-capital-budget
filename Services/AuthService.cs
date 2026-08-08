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
/// Login: SCV-style admin config credentials OR PF + AD + captcha → JWT → USER_TOKEN.
/// Maker/Checker must exist in APP_USER and staff master (Oracle STAFF_DETAILS / Organisations StaffDetails).
/// </summary>
public class AuthService : IAuthService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IStaffLookupService _staffLookup;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthService> _logger;
    private string? _expectedCaptchaAnswer;
    private LoggedInUserDto? _currentUser;

    public AuthService(
        IDbContextFactory<AppDbContext> dbFactory,
        IStaffLookupService staffLookup,
        IConfiguration config,
        IHttpClientFactory httpClientFactory,
        ILogger<AuthService> logger)
    {
        _dbFactory = dbFactory;
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
        await using var db = await _dbFactory.CreateDbContextAsync();
        var questions = await db.LoginCaptchaQuestions
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
        var password = request.Password;

        // SCV pattern: single admin userid + password from ApiKey (not AD, not APP_USER)
        if (IsConfiguredAdminCredentials(pf, password))
            return await CompleteAdminLoginAsync(pf);

        await using var db = await _dbFactory.CreateDbContextAsync();

        var appUser = await db.AppUsers
            .Include(u => u.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PfNo == pf && u.IsActive == "Y");

        if (appUser == null)
        {
            _logger.LogWarning("Login failed — PF {Pf} not in APP_USER", pf);
            return Fail("User is not registered in APP_USER for this application.");
        }

        // Admin role only via config credentials (SCV). Block APP_USER ADMIN rows.
        if (string.Equals(appUser.RoleCode, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Login blocked — APP_USER ADMIN {Pf}; use configured admin login", pf);
            return Fail("Admin must sign in with the configured admin userid and password.");
        }

        var bypassAd = _config.GetValue("Auth:BypassAd", false);
        var adOk = bypassAd || await ValidateAgainstAdAsync(pf, password);
        if (!adOk)
        {
            _logger.LogWarning("Login failed — AD auth rejected for PF {Pf}", pf);
            return Fail("AD authentication failed. Invalid PF or password.");
        }

        var staff = await _staffLookup.LookupByPfAsync(pf);
        if (staff == null)
        {
            _logger.LogWarning("Login failed — PF {Pf} not in staff master (EMPLID)", pf);
            return Fail(string.Format(AppConstants.StaffRequiredForLoginMessage, pf));
        }

        var displayName = staff.EmpName;
        var designation = staff.Designation ?? appUser.Designation;

        return await CompleteUserLoginAsync(db, appUser, pf, displayName, designation,
            appUser.Department?.DeptName, appUser.Department?.HasRevenue == "Y");
    }

    private async Task<LoginResultDto> CompleteAdminLoginAsync(string adminUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var encryptedUserId = EncryptoData.EncryptString(adminUserId);
        var existing = await db.UserTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == encryptedUserId);
        if (existing != null)
        {
            _logger.LogWarning("Login blocked — previous session for admin {User}", adminUserId);
            return Fail(AppConstants.PreviousSessionExistsMessage);
        }

        const string displayName = "Administrator";
        var token = CreateJwt(adminUserId, AppConstants.Roles.Admin, displayName);
        var hash = EncryptoData.Sha256Base64(token);

        try
        {
            db.UserTokens.Add(new UserToken
            {
                UserId = encryptedUserId,
                UserName = displayName,
                LastToken = token,
                HashToken = hash,
                CreatedAt = DateTime.Now
            });
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Login blocked — concurrent USER_TOKEN for admin {User}", adminUserId);
            return Fail(AppConstants.PreviousSessionExistsMessage);
        }

        var user = new LoggedInUserDto
        {
            UserId = 0,
            PfNo = adminUserId,
            UserName = displayName,
            RoleCode = AppConstants.Roles.Admin,
            DeptId = null,
            DeptName = null,
            Designation = "Admin",
            HasRevenueDept = true,
            Token = token,
            TokenHash = hash
        };
        _currentUser = user;
        _logger.LogInformation("Admin login success User={User}", adminUserId);
        return new LoginResultDto
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = user
        };
    }

    private async Task<LoginResultDto> CompleteUserLoginAsync(
        AppDbContext db,
        AppUser appUser,
        string pf,
        string displayName,
        string? designation,
        string? deptName,
        bool hasRevenue)
    {
        var encryptedUserId = EncryptoData.EncryptString(pf);
        var existing = await db.UserTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == encryptedUserId);
        if (existing != null)
        {
            _logger.LogWarning(
                "Login blocked — previous session exists for PF {Pf} (USER_TOKEN REF_NO={Ref})",
                pf, existing.RefNo);
            return Fail(AppConstants.PreviousSessionExistsMessage);
        }

        var token = CreateJwt(pf, appUser.RoleCode, displayName);
        var hash = EncryptoData.Sha256Base64(token);

        try
        {
            db.UserTokens.Add(new UserToken
            {
                UserId = encryptedUserId,
                UserName = displayName,
                LastToken = token,
                HashToken = hash,
                CreatedAt = DateTime.Now
            });
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Login blocked — concurrent USER_TOKEN insert for PF {Pf}", pf);
            return Fail(AppConstants.PreviousSessionExistsMessage);
        }

        var bypassAd = _config.GetValue("Auth:BypassAd", false);
        var user = new LoggedInUserDto
        {
            UserId = appUser.UserId,
            PfNo = appUser.PfNo,
            UserName = displayName,
            RoleCode = appUser.RoleCode,
            DeptId = appUser.DeptId,
            DeptName = deptName,
            Designation = designation,
            HasRevenueDept = hasRevenue,
            Token = token,
            TokenHash = hash
        };
        _currentUser = user;

        _logger.LogInformation("Login success PF={Pf} Role={Role} DeptId={DeptId} BypassAd={Bypass}",
            user.PfNo, user.RoleCode, user.DeptId, bypassAd);

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
        if (string.IsNullOrWhiteSpace(pfNo)) return;

        var encryptedUserId = EncryptoData.EncryptString(pfNo.Trim());
        await using var db = await _dbFactory.CreateDbContextAsync();
        var tokens = await db.UserTokens.Where(t => t.UserId == encryptedUserId).ToListAsync();
        if (tokens.Count > 0)
        {
            db.UserTokens.RemoveRange(tokens);
            await db.SaveChangesAsync();
            _logger.LogInformation("USER_TOKEN cleared for PF={Pf} Rows={Count}", pfNo, tokens.Count);
        }
        _currentUser = null;
    }

    public async Task<LoggedInUserDto?> ValidateSessionAsync(string pfNo, string? tokenHash)
    {
        if (string.IsNullOrWhiteSpace(pfNo) || string.IsNullOrWhiteSpace(tokenHash))
            return null;

        var pf = pfNo.Trim();
        var encryptedUserId = EncryptoData.EncryptString(pf);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var tokenRow = await db.UserTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == encryptedUserId);

        if (tokenRow == null || string.IsNullOrEmpty(tokenRow.LastToken) || string.IsNullOrEmpty(tokenRow.HashToken))
        {
            _logger.LogInformation("Session invalid for PF {Pf} — USER_TOKEN missing", pf);
            return null;
        }

        if (!string.Equals(tokenRow.HashToken, tokenHash.Trim(), StringComparison.Ordinal))
        {
            _logger.LogWarning("Session hash mismatch for PF {Pf} — treating as logged out", pf);
            return null;
        }

        if (IsConfiguredAdminUserId(pf))
        {
            return new LoggedInUserDto
            {
                UserId = 0,
                PfNo = pf,
                UserName = tokenRow.UserName ?? "Administrator",
                RoleCode = AppConstants.Roles.Admin,
                DeptId = null,
                DeptName = null,
                Designation = "Admin",
                HasRevenueDept = true,
                Token = tokenRow.LastToken,
                TokenHash = tokenRow.HashToken
            };
        }

        var appUser = await db.AppUsers
            .Include(u => u.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PfNo == pf && u.IsActive == "Y");
        if (appUser == null) return null;

        if (string.Equals(appUser.RoleCode, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            return null;

        var staff = await _staffLookup.LookupByPfAsync(pf);
        var displayName = staff?.EmpName ?? appUser.UserName ?? pf;

        return new LoggedInUserDto
        {
            UserId = appUser.UserId,
            PfNo = appUser.PfNo,
            UserName = displayName,
            RoleCode = appUser.RoleCode,
            DeptId = appUser.DeptId,
            DeptName = appUser.Department?.DeptName,
            Designation = staff?.Designation ?? appUser.Designation,
            HasRevenueDept = appUser.Department?.HasRevenue == "Y",
            Token = tokenRow.LastToken,
            TokenHash = tokenRow.HashToken
        };
    }

    public async Task<bool> IsCurrentSessionValidAsync()
    {
        var current = _currentUser;
        if (current == null || string.IsNullOrWhiteSpace(current.TokenHash))
            return false;

        var validated = await ValidateSessionAsync(current.PfNo, current.TokenHash);
        if (validated == null)
        {
            _currentUser = null;
            return false;
        }

        _currentUser = validated;
        return true;
    }

    public Task<LoggedInUserDto?> GetLoggedInUserByPfAsync(string pfNo) =>
        ValidateSessionAsync(pfNo, _currentUser?.TokenHash);

    private bool IsConfiguredAdminCredentials(string userId, string password)
    {
        try
        {
            var adminId = DecryptConfigValue(_config["ApiKey:ADMIN_USER_ID"]);
            var adminPwd = DecryptConfigValue(_config["ApiKey:PWD"]);
            if (string.IsNullOrWhiteSpace(adminId) || string.IsNullOrWhiteSpace(adminPwd))
                return false;
            return string.Equals(userId.Trim(), adminId.Trim(), StringComparison.Ordinal)
                   && string.Equals(password, adminPwd, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt ApiKey ADMIN credentials");
            return false;
        }
    }

    private bool IsConfiguredAdminUserId(string userId)
    {
        try
        {
            var adminId = DecryptConfigValue(_config["ApiKey:ADMIN_USER_ID"]);
            return !string.IsNullOrWhiteSpace(adminId)
                   && string.Equals(userId.Trim(), adminId.Trim(), StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    private static string? DecryptConfigValue(string? configured)
    {
        if (string.IsNullOrWhiteSpace(configured))
            return null;
        var raw = configured.Trim().Replace(" ", "+");
        if (raw.StartsWith("U2FsdGVk", StringComparison.Ordinal) ||
            raw.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
        {
            if (raw.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                raw = raw["ENC:".Length..];
            return EncryptoData.DecryptAes(raw);
        }
        return configured.Trim();
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
