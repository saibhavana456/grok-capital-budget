namespace IT_BUDGET_MONITORING_PORTAL.Helpers;

public static class AppConstants
{
    public static readonly string[] FyMonths =
    [
        "April", "May", "June", "July", "August", "September",
        "October", "November", "December", "January", "February", "March"
    ];

    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string Maker = "MAKER";
        public const string Checker = "CHECKER";
    }

    public static class EntryStatus
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Returned = "RETURNED";
    }

    public const string OverBudgetMessage =
        "Entered budget utilization exceeds the allocated budget. Please enter a valid amount within the approved budget limit.";

    public const int JustificationMaxLength = 5000;
    public const string DefaultFinancialYear = "2026-27";
    public const string SessionUserKey = "IT_BUDGET_USER";

    /// <summary>Previous month in Indian FY order (April…March).</summary>
    public static string PreviousMonth(string month)
    {
        var idx = Array.FindIndex(FyMonths, m => string.Equals(m, month, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return month;
        return idx == 0 ? FyMonths[^1] : FyMonths[idx - 1];
    }

    /// <summary>Next month in Indian FY order (April…March).</summary>
    public static string NextMonth(string month)
    {
        var idx = Array.FindIndex(FyMonths, m => string.Equals(m, month, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return month;
        return idx >= FyMonths.Length - 1 ? FyMonths[0] : FyMonths[idx + 1];
    }
}
