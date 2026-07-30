using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using IT_BUDGET_MONITORING_PORTAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

public class RevenueService : IRevenueService
{
    private readonly AppDbContext _db;
    private readonly ILogger<RevenueService> _logger;

    public RevenueService(AppDbContext db, ILogger<RevenueService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<RevenueEntryFormDto?> BuildFormAsync(long sectionId, string financialYear, string entryMonth)
    {
        var section = await _db.Sections.Include(s => s.Department).AsNoTracking()
            .FirstOrDefaultAsync(s => s.SectionId == sectionId && s.IsActive == "Y");
        if (section?.Department == null || section.Department.HasRevenue != "Y")
            return null;

        var allotment = await _db.SectionFyRevenueAllotments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.SectionId == sectionId && a.FinancialYear == financialYear && a.IsActive == "Y");

        var heads = await _db.RevenueHeads.AsNoTracking()
            .Where(h => h.IsActive == "Y")
            .OrderBy(h => h.DisplayOrder)
            .ToListAsync();

        var prevMonth = AppConstants.PreviousMonth(entryMonth);
        var prevEntry = await _db.RevenueMonthlyEntries.AsNoTracking()
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.SectionId == sectionId
                                      && e.FinancialYear == financialYear
                                      && e.EntryMonth == prevMonth
                                      && e.IsActive == "Y"
                                      && e.EntryStatus == AppConstants.EntryStatus.Approved);

        var prevAmounts = prevEntry?.Lines.ToDictionary(l => l.HeadId, l => l.Amount)
                          ?? new Dictionary<long, decimal>();

        var utilizedTillPrev = await SumApprovedUtilizedThroughAsync(sectionId, financialYear, prevMonth);

        var form = new RevenueEntryFormDto
        {
            SectionId = sectionId,
            FinancialYear = financialYear,
            EntryMonth = entryMonth,
            SectionName = section.SectionName,
            DeptName = section.Department.DeptName,
            TotalAllotted = allotment?.TotalAllotted ?? 0,
            UtilizedTillPreviousMonth = utilizedTillPrev,
            PrevTotal = prevAmounts.Values.Sum(),
            PrevLines = heads.Select(h => new RevenueLineDto
            {
                HeadId = h.HeadId,
                HeadName = h.HeadName,
                Amount = prevAmounts.TryGetValue(h.HeadId, out var amt) ? amt : 0
            }).ToList(),
            Lines = heads.Select(h => new RevenueLineDto
            {
                HeadId = h.HeadId,
                HeadName = h.HeadName,
                Amount = 0
            }).ToList()
        };

        var editable = await _db.RevenueMonthlyEntries.AsNoTracking()
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.SectionId == sectionId
                                      && e.FinancialYear == financialYear
                                      && e.EntryMonth == entryMonth
                                      && e.IsActive == "Y");
        if (editable != null)
        {
            form.JustificationText = editable.JustificationText ?? "";
            var amounts = editable.Lines.ToDictionary(l => l.HeadId, l => l.Amount);
            foreach (var line in form.Lines)
                if (amounts.TryGetValue(line.HeadId, out var amt))
                    line.Amount = amt;
        }

        return form;
    }

    public async Task<ExistingEntryInfo?> FindActiveEntryAsync(long sectionId, string financialYear, string entryMonth)
    {
        return await _db.RevenueMonthlyEntries.AsNoTracking()
            .Where(e => e.SectionId == sectionId
                        && e.FinancialYear == financialYear
                        && e.EntryMonth == entryMonth
                        && e.IsActive == "Y")
            .Select(e => new ExistingEntryInfo
            {
                EntryId = e.EntryId,
                Status = e.EntryStatus,
                SubmittedByPf = e.SubmittedByPf,
                CheckerRemark = e.CheckerRemark
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult> SubmitAsync(RevenueEntryFormDto form, string makerPf)
    {
        if (form.SectionId <= 0) return ServiceResult.Fail("Section is required.");
        if (string.IsNullOrWhiteSpace(form.EntryMonth)) return ServiceResult.Fail("Month is required.");
        if (!AppConstants.IsCurrentFinancialYear(form.FinancialYear))
            return ServiceResult.Fail(AppConstants.PreviousFyViewOnlyMessage);
        if (AppConstants.IsFutureMonth(form.EntryMonth))
            return ServiceResult.Fail("Future month entry is not allowed.");
        if (string.IsNullOrWhiteSpace(form.JustificationText))
            return ServiceResult.Fail(AppConstants.JustificationRequiredMessage);
        if (form.JustificationText.Length > AppConstants.JustificationMaxLength)
            return ServiceResult.Fail($"Justification cannot exceed {AppConstants.JustificationMaxLength} characters.");

        var dup = form.Lines.GroupBy(l => l.HeadId).Any(g => g.Count() > 1);
        if (dup) return ServiceResult.Fail("Duplicate expenditure head is not allowed.");

        var section = await _db.Sections.Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.SectionId == form.SectionId);
        if (section?.Department?.HasRevenue != "Y")
            return ServiceResult.Fail("Revenue entry is allowed only for DIT departments with revenue enabled.");

        var allotment = await _db.SectionFyRevenueAllotments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.SectionId == form.SectionId && a.FinancialYear == form.FinancialYear && a.IsActive == "Y");
        if (allotment == null)
            return ServiceResult.Fail("Revenue FY allotment not found for this section.");

        foreach (var line in form.Lines)
            line.Amount = Math.Max(0, line.Amount);

        var existing = await _db.RevenueMonthlyEntries
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.SectionId == form.SectionId
                                      && e.FinancialYear == form.FinancialYear
                                      && e.EntryMonth == form.EntryMonth
                                      && e.IsActive == "Y");

        if (existing != null)
        {
            if (AppConstants.IsLockedStatus(existing.EntryStatus))
                return ServiceResult.Fail("An entry already exists for this section, FY and month.");

            if (!AppConstants.IsEditableStatus(existing.EntryStatus))
                return ServiceResult.Fail("This entry cannot be resubmitted.");

            if (!AppConstants.IsAllowedEntryMonth(form.EntryMonth))
                return ServiceResult.Fail(AppConstants.EntryMonthWindowMessage);

            existing.JustificationText = form.JustificationText.Trim();
            existing.EntryStatus = AppConstants.EntryStatus.Pending;
            existing.SubmittedAt = DateTime.Now;
            existing.SubmittedByPf = makerPf;
            existing.CheckedAt = null;
            existing.CheckedByPf = null;
            existing.CheckerRemark = null;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = makerPf;

            _db.RevenueMonthlyEntryLines.RemoveRange(existing.Lines);
            foreach (var line in form.Lines)
            {
                _db.RevenueMonthlyEntryLines.Add(new RevenueMonthlyEntryLine
                {
                    EntryId = existing.EntryId,
                    HeadId = line.HeadId,
                    Amount = line.Amount
                });
            }

            await _db.SaveChangesAsync();
            var resubmitTotal = form.Lines.Sum(l => l.Amount);
            _logger.LogInformation("Revenue resubmit EntryId={EntryId} SectionId={SectionId} Month={Month} PF={Pf} Total={Total}",
                existing.EntryId, form.SectionId, form.EntryMonth, makerPf, resubmitTotal);
            return ServiceResult.Ok(AppConstants.SuccessResubmit);
        }

        if (!AppConstants.CanSubmitNewEntry(form.FinancialYear, form.EntryMonth))
            return ServiceResult.Fail(AppConstants.EntryMonthWindowMessage);

        var entry = new RevenueMonthlyEntry
        {
            SectionId = form.SectionId,
            FinancialYear = form.FinancialYear,
            EntryMonth = form.EntryMonth,
            JustificationText = form.JustificationText.Trim(),
            EntryStatus = AppConstants.EntryStatus.Pending,
            SubmittedAt = DateTime.Now,
            SubmittedByPf = makerPf,
            IsActive = "Y",
            CreatedAt = DateTime.Now,
            CreatedBy = makerPf
        };
        _db.RevenueMonthlyEntries.Add(entry);
        await _db.SaveChangesAsync();

        foreach (var line in form.Lines)
        {
            _db.RevenueMonthlyEntryLines.Add(new RevenueMonthlyEntryLine
            {
                EntryId = entry.EntryId,
                HeadId = line.HeadId,
                Amount = line.Amount
            });
        }
        await _db.SaveChangesAsync();
        var submitTotal = form.Lines.Sum(l => l.Amount);
        _logger.LogInformation("Revenue submit EntryId={EntryId} SectionId={SectionId} Month={Month} PF={Pf} Total={Total}",
            entry.EntryId, form.SectionId, form.EntryMonth, makerPf, submitTotal);
        return ServiceResult.Ok(AppConstants.SuccessSubmit);
    }

    public async Task<List<RevenueSubmissionListItem>> GetSubmissionsAsync(
        string? makerPfFilter, long? deptIdFilter, string? checkerPfFilter = null)
    {
        var q = from e in _db.RevenueMonthlyEntries.AsNoTracking()
                join s in _db.Sections.AsNoTracking() on e.SectionId equals s.SectionId
                join d in _db.Departments.AsNoTracking() on s.DeptId equals d.DeptId
                where e.IsActive == "Y"
                select new { e, s, d };

        if (!string.IsNullOrWhiteSpace(makerPfFilter))
            q = q.Where(x => x.e.SubmittedByPf == makerPfFilter);

        if (!string.IsNullOrWhiteSpace(checkerPfFilter))
        {
            var checkerSectionIds = await ResolveRevenueSectionIdsForCheckerAsync(checkerPfFilter.Trim());
            q = q.Where(x => checkerSectionIds.Contains(x.e.SectionId));
        }
        else if (deptIdFilter.HasValue)
        {
            q = q.Where(x => x.s.DeptId == deptIdFilter.Value);
        }

        var rows = await q.OrderByDescending(x => x.e.SubmittedAt).ToListAsync();
        var result = new List<RevenueSubmissionListItem>();
        foreach (var row in rows)
        {
            var total = await _db.RevenueMonthlyEntryLines.AsNoTracking()
                .Where(l => l.EntryId == row.e.EntryId)
                .SumAsync(l => (decimal?)l.Amount) ?? 0;
            result.Add(new RevenueSubmissionListItem
            {
                EntryId = row.e.EntryId,
                SectionId = row.e.SectionId,
                FinancialYear = row.e.FinancialYear,
                Month = row.e.EntryMonth,
                DeptName = row.d.DeptName,
                SectionName = row.s.SectionName,
                TotalAmount = total,
                Status = row.e.EntryStatus,
                SubmittedByPf = row.e.SubmittedByPf
            });
        }
        return result;
    }

    public Task<RevenueMonthlyEntry?> GetEntryAsync(long entryId) =>
        _db.RevenueMonthlyEntries
            .Include(e => e.Section)
            .Include(e => e.Lines).ThenInclude(l => l.Head)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EntryId == entryId);

    public async Task<List<RevenueSubmissionListItem>> GetPendingForCheckerAsync(string checkerPf)
    {
        var pfNorm = (checkerPf ?? "").Trim();
        if (string.IsNullOrEmpty(pfNorm))
            return new List<RevenueSubmissionListItem>();

        var sectionIds = await ResolveRevenueSectionIdsForCheckerAsync(pfNorm);
        if (sectionIds.Count == 0)
        {
            _logger.LogWarning(
                "Revenue pending empty — no section/dept CHECKER_PF match for {Pf}",
                pfNorm);
            return new List<RevenueSubmissionListItem>();
        }

        var q = from e in _db.RevenueMonthlyEntries.AsNoTracking()
                join s in _db.Sections.AsNoTracking() on e.SectionId equals s.SectionId
                join d in _db.Departments.AsNoTracking() on s.DeptId equals d.DeptId
                where e.IsActive == "Y"
                      && e.EntryStatus == AppConstants.EntryStatus.Pending
                      && sectionIds.Contains(e.SectionId)
                orderby e.SubmittedAt descending
                select new { e, s, d };

        var rows = await q.ToListAsync();
        var result = new List<RevenueSubmissionListItem>();
        foreach (var row in rows)
        {
            var total = await _db.RevenueMonthlyEntryLines.AsNoTracking()
                .Where(l => l.EntryId == row.e.EntryId)
                .SumAsync(l => (decimal?)l.Amount) ?? 0;
            result.Add(new RevenueSubmissionListItem
            {
                EntryId = row.e.EntryId,
                SectionId = row.e.SectionId,
                FinancialYear = row.e.FinancialYear,
                Month = row.e.EntryMonth,
                DeptName = row.d.DeptName,
                SectionName = row.s.SectionName,
                TotalAmount = total,
                Status = row.e.EntryStatus,
                SubmittedByPf = row.e.SubmittedByPf
            });
        }
        return result;
    }

    private async Task<List<long>> ResolveRevenueSectionIdsForCheckerAsync(string checkerPf)
    {
        var rows = await (
            from s in _db.Sections.AsNoTracking()
            join d in _db.Departments.AsNoTracking() on s.DeptId equals d.DeptId
            where s.IsActive == "Y" && d.IsActive == "Y" && d.HasRevenue == "Y"
            select new { s.SectionId, SectionChecker = s.CheckerPf, d.DeptCode, DeptChecker = d.CheckerPf }
        ).ToListAsync();

        return rows
            .Where(x =>
                PfEquals(x.SectionChecker, checkerPf)
                || PfEquals(x.DeptChecker, checkerPf))
            .Select(x => x.SectionId)
            .Distinct()
            .ToList();
    }

    private static bool PfEquals(string? a, string b) =>
        !string.IsNullOrWhiteSpace(a)
        && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private async Task<List<long>> GetActiveDeptIdsForCheckerAsync(string checkerPf)
    {
        var pfNorm = (checkerPf ?? "").Trim();
        if (string.IsNullOrEmpty(pfNorm)) return new List<long>();

        var deptIds = await _db.Departments.AsNoTracking()
            .Where(d => d.IsActive == "Y")
            .Select(d => new { d.DeptId, d.CheckerPf })
            .ToListAsync();

        var fromDept = deptIds.Where(d => PfEquals(d.CheckerPf, pfNorm)).Select(d => d.DeptId);
        var fromSection = await _db.Sections.AsNoTracking()
            .Where(s => s.IsActive == "Y" && s.CheckerPf != null)
            .Select(s => new { s.DeptId, s.CheckerPf })
            .ToListAsync();

        return fromDept
            .Concat(fromSection.Where(s => PfEquals(s.CheckerPf, pfNorm)).Select(s => s.DeptId))
            .Distinct()
            .ToList();
    }

    public async Task<ServiceResult> CheckerActionAsync(long entryId, string action, string checkerPf, string? remark)
    {
        var entry = await _db.RevenueMonthlyEntries.FirstOrDefaultAsync(e => e.EntryId == entryId && e.IsActive == "Y");
        if (entry == null) return ServiceResult.Fail("Entry not found.");
        if (entry.EntryStatus != AppConstants.EntryStatus.Pending)
            return ServiceResult.Fail("Only PENDING entries can be actioned.");

        var section = await _db.Sections.Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.SectionId == entry.SectionId);
        if (section?.Department == null)
            return ServiceResult.Fail("You are not the Checker for this section.");

        var allowed = PfEquals(section.CheckerPf, checkerPf)
                      || PfEquals(section.Department.CheckerPf, checkerPf);
        if (!allowed)
            return ServiceResult.Fail("You are not the Checker for this section/department.");

        var normalized = action.Trim().ToUpperInvariant();
        if (normalized is not (AppConstants.EntryStatus.Approved or AppConstants.EntryStatus.Rejected or AppConstants.EntryStatus.Returned))
            return ServiceResult.Fail("Invalid checker action.");

        if ((normalized == AppConstants.EntryStatus.Rejected || normalized == AppConstants.EntryStatus.Returned)
            && string.IsNullOrWhiteSpace(remark))
            return ServiceResult.Fail(AppConstants.RemarkRequiredMessage);

        entry.EntryStatus = normalized;
        entry.CheckedAt = DateTime.Now;
        entry.CheckedByPf = checkerPf;
        entry.CheckerRemark = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim();
        entry.UpdatedAt = DateTime.Now;
        entry.UpdatedBy = checkerPf;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Revenue checker action EntryId={EntryId} Action={Action} CheckerPf={Pf}",
            entryId, normalized, checkerPf);

        return ServiceResult.Ok(normalized switch
        {
            AppConstants.EntryStatus.Approved => AppConstants.SuccessApprove,
            AppConstants.EntryStatus.Returned => AppConstants.SuccessReturn,
            _ => AppConstants.SuccessReject
        });
    }

    private async Task<decimal> SumApprovedUtilizedThroughAsync(long sectionId, string financialYear, string throughMonth)
    {
        var months = AppConstants.MonthsFromAprilThrough(throughMonth).ToList();
        if (months.Count == 0) return 0;

        var entryIds = await _db.RevenueMonthlyEntries.AsNoTracking()
            .Where(e => e.SectionId == sectionId
                        && e.FinancialYear == financialYear
                        && e.IsActive == "Y"
                        && e.EntryStatus == AppConstants.EntryStatus.Approved
                        && months.Contains(e.EntryMonth))
            .Select(e => e.EntryId)
            .ToListAsync();

        if (entryIds.Count == 0) return 0;

        return await _db.RevenueMonthlyEntryLines.AsNoTracking()
            .Where(l => entryIds.Contains(l.EntryId))
            .SumAsync(l => (decimal?)l.Amount) ?? 0;
    }
}
