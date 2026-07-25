namespace IT_BUDGET_MONITORING_PORTAL.Models.DTOs;

public class LoginRequestDto
{
    public string PfNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CaptchaAnswer { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public LoggedInUserDto? User { get; set; }
}

public class LoggedInUserDto
{
    public long UserId { get; set; }
    public string PfNo { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public long? DeptId { get; set; }
    public string? DeptName { get; set; }
    public string? Designation { get; set; }
    public bool HasRevenueDept { get; set; }
    public string Token { get; set; } = string.Empty;
    /// <summary>SHA-256 of Token — stored in cookie and matched to USER_TOKEN.HASH_TOKEN.</summary>
    public string TokenHash { get; set; } = string.Empty;
}

public class CaptchaQuestionDto
{
    public long QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
}

public class CapitalSubmissionListItem
{
    public long EntryId { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? SubmittedByPf { get; set; }
}

public class RevenueSubmissionListItem
{
    public long EntryId { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public string Month { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SubmittedByPf { get; set; }
}

public class CapitalEntryFormDto
{
    public long ProjectId { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public string EntryMonth { get; set; } = string.Empty;
    public decimal SpilloverAllotted { get; set; }
    public decimal FreshAllotted { get; set; }
    public decimal TotalAllotted { get; set; }
    /// <summary>Cumulative approved utilization from April through the month before entry month.</summary>
    public decimal UtilizedTillPreviousMonth { get; set; }
    public decimal PrevSpillover { get; set; }
    public decimal PrevFresh { get; set; }
    public decimal PrevTotal { get; set; }
    public decimal ActualSpillover { get; set; }
    public decimal ActualFresh { get; set; }
    public decimal EstSpilloverNext { get; set; }
    public decimal EstFreshNext { get; set; }
    public string JustificationText { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
}

public class RevenueLineDto
{
    public long HeadId { get; set; }
    public string HeadName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class RevenueEntryFormDto
{
    public long SectionId { get; set; }
    public string FinancialYear { get; set; } = string.Empty;
    public string EntryMonth { get; set; } = string.Empty;
    public decimal TotalAllotted { get; set; }
    /// <summary>Cumulative approved utilization from April through the month before entry month.</summary>
    public decimal UtilizedTillPreviousMonth { get; set; }
    public string JustificationText { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public decimal PrevTotal { get; set; }
    public List<RevenueLineDto> PrevLines { get; set; } = new();
    public List<RevenueLineDto> Lines { get; set; } = new();
}

public class ExistingEntryInfo
{
    public long EntryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SubmittedByPf { get; set; }
    public string? CheckerRemark { get; set; }
}

public class ServiceResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ServiceResult Ok(string message = "OK") => new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => new() { Success = false, Message = message };
}
