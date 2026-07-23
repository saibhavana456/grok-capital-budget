using System.Globalization;

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

    /// <summary>Calendar month name matching FyMonths (e.g. July).</summary>
    public static string CalendarMonthName(DateTime? asOf = null)
    {
        var dt = asOf ?? DateTime.Now;
        return dt.ToString("MMMM", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Entry months allowed for Maker entry: calendar previous + current only.
    /// Next-month figures are estimates on the same form — not a selectable entry month.
    /// </summary>
    public static IReadOnlyList<string> AllowedEntryMonths(DateTime? asOf = null)
    {
        var current = CalendarMonthName(asOf);
        var previous = PreviousMonth(current);
        return new[] { previous, current };
    }

    public static bool IsAllowedEntryMonth(string? month, DateTime? asOf = null)
    {
        if (string.IsNullOrWhiteSpace(month)) return false;
        return AllowedEntryMonths(asOf)
            .Any(m => string.Equals(m, month, StringComparison.OrdinalIgnoreCase));
    }

    public static string DefaultEntryMonth(DateTime? asOf = null) => CalendarMonthName(asOf);

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
