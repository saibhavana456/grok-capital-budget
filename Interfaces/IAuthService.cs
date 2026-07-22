using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;

namespace IT_BUDGET_MONITORING_PORTAL.Interfaces;

public interface IAuthService
{
    Task<CaptchaQuestionDto> GetCaptchaAsync();
    Task<LoginResultDto> LoginAsync(LoginRequestDto request, string expectedCaptchaAnswer);
    Task LogoutAsync(string pfNo);
    LoggedInUserDto? CurrentUser { get; }
    void SetCurrentUser(LoggedInUserDto? user);
    bool IsAuthenticated { get; }
}
