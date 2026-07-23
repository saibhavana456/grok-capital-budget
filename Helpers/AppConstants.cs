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

    /// <summary>
    /// True when the FY month is after the calendar month (future — not selectable for entry/view navigate).
    /// Uses Indian FY year mapping (April–March).
    /// </summary>
    public static bool IsFutureMonth(string? month, DateTime? asOf = null)
    {
        if (string.IsNullOrWhiteSpace(month)) return true;
        var now = asOf ?? DateTime.Now;
        if (!TryFyMonthDate(month, now, out var monthDate)) return true;
        var current = new DateTime(now.Year, now.Month, 1);
        return monthDate > current;
    }

    /// <summary>Past and current FY months may be opened; future months stay disabled.</summary>
    public static bool CanOpenMonth(string? month, DateTime? asOf = null) =>
        !string.IsNullOrWhiteSpace(month) && !IsFutureMonth(month, asOf);

    private static bool TryFyMonthDate(string month, DateTime asOf, out DateTime result)
    {
        result = default;
        if (!DateTime.TryParseExact(month.Trim(), "MMMM", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
            return false;

        var monthNum = parsed.Month;
        var fyStartYear = asOf.Month >= 4 ? asOf.Year : asOf.Year - 1;
        var year = monthNum >= 4 ? fyStartYear : fyStartYear + 1;
        result = new DateTime(year, monthNum, 1);
        return true;
    }

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
