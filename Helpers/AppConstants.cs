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
        "Entered amount exceeds remaining allotment. You can still submit — checker will review.";

    public const string SuccessSubmit = "Budget entry submitted successfully and is pending checker review.";
    public const string SuccessResubmit = "Budget entry updated and resubmitted for checker review.";
    public const string SuccessApprove = "Submission approved successfully.";
    public const string SuccessReturn = "Submission returned to maker for correction.";
    public const string SuccessReject = "Submission rejected.";

    public const string ConfirmSubmitTitle = "Confirm submission";
    public const string ConfirmSubmitMessage =
        "Do you want to submit this budget entry for checker review?";
    public const string ConfirmZeroTitle = "Confirm zero amounts";
    public const string ConfirmZeroMessage =
        "All amount fields are zero. Do you still want to submit?";
    /// <summary>Capital: zero actual utilization is not allowed. Revenue may submit zeros with confirm.</summary>
    public const string CapitalZeroNotAllowedMessage =
        "Capital entry cannot be submitted with zero actual utilization. Enter spillover and/or fresh amount greater than zero.";
    /// <summary>Priyadarshini 30-Jul-2026: Capital must not submit when over allotted budget.</summary>
    public const string CapitalOverBudgetBlockedTitle = "Allocated budget exceeded";
    public const string CapitalOverBudgetBlockedMessage =
        "You have exceeded the allocated budget. Kindly request for additional budget. Capital entry cannot be submitted.";
    /// <summary>Priyadarshini 30-Jul-2026: Revenue over-allotment is informational; submit still allowed.</summary>
    public const string RevenueOverBudgetInfoTitle = "Allocated budget exceeded";
    public const string RevenueOverBudgetInfoMessage =
        "The amount you have entered has exceeded the allocated budget of the respective section. This is for your information.";
    public const string MakerPfRequiredMessage = "Maker PF is required.";
    public const string CheckerPfRequiredMessage = "Checker PF is required.";
    public const string MakerNotInAppUserMessage =
        "Maker PF '{0}' is not registered as an active Maker or Checker in APP_USER.";
    public const string CheckerNotInAppUserMessage =
        "Checker PF '{0}' is not registered as an active Maker or Checker in APP_USER.";
    public const string AdminCannotBeMakerOrCheckerMessage =
        "PF '{0}' is ADMIN and cannot be assigned as Maker or Checker.";
    public const string StaffNotInOrganisationsMessage =
        "PF '{0}' was not found in Organisations StaffDetails. Confirm the PF with HR data before assigning.";
    public const string OrganisationsNotConfiguredMessage =
        "Organisations DB is not configured — staff name/email/scale lookup is skipped (APP_USER validation still applies).";
    public const string MakerScaleInvalidMessage =
        "Maker PF '{0}' must be Scale 1 to 4 (Organisations EMP_SCALE_CODE). Found: {1}.";
    public const string CheckerScaleInvalidMessage =
        "Checker PF '{0}' must be Scale 4 or above (Organisations EMP_SCALE_CODE). Found: {1}.";
    public const string MakerCheckerSamePfMessage =
        "Maker PF and Checker PF must be different for the same project/section/department.";
    public const string PfAlreadyOnOtherDeptMessage =
        "PF {0} is already assigned as {1} on department '{2}'. One person can belong to only one active department.";
    public const string ConfirmOverBudgetTitle = "Amount exceeds allotment";
    public const string ConfirmOverBudgetMessage =
        "The amount you have entered has exceeded the allocated budget of the respective section. This is for your information. Do you still want to submit?";
    public const string RemarkRequiredMessage = "Remark is required for Return and Reject.";
    public const string JustificationRequiredMessage = "Justification is required.";

    /// <summary>DIT uses project-level (capital) and section-level (revenue) Maker/Checker.</summary>
    public const string DitDeptCode = "DIT";
    public const int MakerScaleMin = 1;
    public const int MakerScaleMax = 4;
    public const int CheckerScaleMin = 4;
    /// <summary>Same message as Personal/SCV live when USER_TOKEN already exists.</summary>
    public const string PreviousSessionExistsMessage =
        "Please Clear previous Session then try to login again";
    public const string ClearPreviousSessionTitle = "Previous session active";
    public const string ClearPreviousSessionMessage =
        "User already logged in. Clear the previous session, then login again.";

    public const int JustificationMaxLength = 5000;

    /// <summary>Seed/demo default; runtime entry uses <see cref="CurrentFinancialYear"/>.</summary>
    public const string DefaultFinancialYear = "2026-27";

    public const string PreviousFyViewOnlyMessage =
        "Previous financial years are view-only. New entry is allowed only for the current financial year.";
    public const string EntryMonthWindowMessage =
        "New entry is allowed only for the current month or the previous calendar month.";

    public static bool IsEditableStatus(string? status) =>
        string.Equals(status, EntryStatus.Returned, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, EntryStatus.Rejected, StringComparison.OrdinalIgnoreCase);

    public static bool IsLockedStatus(string? status) =>
        string.Equals(status, EntryStatus.Pending, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, EntryStatus.Approved, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Indian FY label for the calendar date (April–March), e.g. 25 Jul 2026 → 2026-27.
    /// </summary>
    public static string CurrentFinancialYear(DateTime? asOf = null)
    {
        var dt = asOf ?? DateTime.Now;
        var startYear = dt.Month >= 4 ? dt.Year : dt.Year - 1;
        var endTwo = (startYear + 1) % 100;
        return $"{startYear}-{endTwo:D2}";
    }

    public static bool IsCurrentFinancialYear(string? financialYear, DateTime? asOf = null)
    {
        if (string.IsNullOrWhiteSpace(financialYear)) return false;
        return string.Equals(financialYear.Trim(), CurrentFinancialYear(asOf), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsDitDepartment(string? deptCode) =>
        string.Equals((deptCode ?? "").Trim(), DitDeptCode, StringComparison.OrdinalIgnoreCase);

    /// <summary>Parse Organisations EMP_SCALE_CODE / description to numeric scale (1–8 typical).</summary>
    public static int? TryParseEmployeeScale(string? scaleCode, string? scaleDescr = null)
    {
        foreach (var raw in new[] { scaleCode, scaleDescr })
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var s = raw.Trim();
            if (int.TryParse(s, out var n) && n is >= 1 and <= 20)
                return n;
            // e.g. "SMGS-IV", "Scale 4", "IV"
            var digits = new string(s.Where(char.IsDigit).ToArray());
            if (digits.Length > 0 && int.TryParse(digits, out n) && n is >= 1 and <= 20)
                return n;
            var roman = s.ToUpperInvariant();
            if (roman.Contains("VIII") || roman.EndsWith("-8") || roman.Contains(" SCALE 8")) return 8;
            if (roman.Contains("VII") || roman.EndsWith("-7")) return 7;
            if (roman.Contains("VI") || roman.EndsWith("-6")) return 6;
            if (roman.Contains("IV") || roman.EndsWith("-4") || roman.Contains(" SCALE 4")) return 4;
            if (roman.Contains("V") || roman.EndsWith("-5")) return 5;
            if (roman.Contains("III") || roman.EndsWith("-3")) return 3;
            if (roman.Contains("II") || roman.EndsWith("-2")) return 2;
            if (roman.Contains('I') || roman.EndsWith("-1")) return 1;
        }
        return null;
    }

    public static bool IsMakerScaleAllowed(int? scale) =>
        scale is >= MakerScaleMin and <= MakerScaleMax;

    public static bool IsCheckerScaleAllowed(int? scale) =>
        scale is >= CheckerScaleMin;

    /// <summary>Previous Indian FY label, e.g. current 2026-27 → 2025-26.</summary>
    public static string PreviousFinancialYear(DateTime? asOf = null)
    {
        var current = CurrentFinancialYear(asOf);
        var start = int.Parse(current.AsSpan(0, 4), CultureInfo.InvariantCulture) - 1;
        return $"{start}-{(start + 1) % 100:D2}";
    }

    /// <summary>FY choices on entry pages: current (editable window) + previous (read-only).</summary>
    public static IReadOnlyList<string> SelectableFinancialYears(DateTime? asOf = null) =>
        new[] { CurrentFinancialYear(asOf), PreviousFinancialYear(asOf) };

    /// <summary>
    /// Month openable on entry/view: current FY uses entry window for edit and all past months for view;
    /// previous FY opens every month (all read-only).
    /// </summary>
    public static bool IsMonthOpenable(string? financialYear, string? month, DateTime? asOf = null)
    {
        if (string.IsNullOrWhiteSpace(month)) return false;
        if (!IsCurrentFinancialYear(financialYear, asOf))
            return FyMonths.Any(m => string.Equals(m, month, StringComparison.OrdinalIgnoreCase));
        return CanOpenMonth(month, asOf);
    }

    /// <summary>
    /// Maker may submit only in the current FY for current or previous calendar month
    /// (or resubmit RETURNED/REJECTED in that same window — enforced by caller with status).
    /// </summary>
    public static bool CanSubmitNewEntry(string? financialYear, string? month, DateTime? asOf = null) =>
        IsCurrentFinancialYear(financialYear, asOf) && IsAllowedEntryMonth(month, asOf);

    /// <summary>FY months from April through <paramref name="throughMonth"/> inclusive.</summary>
    public static IEnumerable<string> MonthsFromAprilThrough(string throughMonth)
    {
        var idx = Array.FindIndex(FyMonths, m => string.Equals(m, throughMonth, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) yield break;
        for (var i = 0; i <= idx; i++)
            yield return FyMonths[i];
    }

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

    /// <summary>
    /// Month selectable on Maker entry/portal: only current + previous (not older completed months, not future).
    /// </summary>
    public static bool IsEntryMonthSelectable(string? month, DateTime? asOf = null) =>
        IsAllowedEntryMonth(month, asOf);

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

    /// <summary>Past and current FY months may be opened for view; future months stay disabled.</summary>
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
