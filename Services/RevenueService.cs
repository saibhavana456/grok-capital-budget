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

    public RevenueService(AppDbContext db) => _db = db;

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

        return new RevenueEntryFormDto
        {
            SectionId = sectionId,
            FinancialYear = financialYear,
            EntryMonth = entryMonth,
            SectionName = section.SectionName,
            DeptName = section.Department.DeptName,
            TotalAllotted = allotment?.TotalAllotted ?? 0,
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
                SubmittedByPf = e.SubmittedByPf
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult> SubmitAsync(RevenueEntryFormDto form, string makerPf)
    {
        if (form.SectionId <= 0) return ServiceResult.Fail("Section is required.");
        if (string.IsNullOrWhiteSpace(form.EntryMonth)) return ServiceResult.Fail("Month is required.");
        if (!AppConstants.IsAllowedEntryMonth(form.EntryMonth))
            return ServiceResult.Fail("Entry is allowed only for the current month or the previous month.");
        if ((form.JustificationText?.Length ?? 0) > AppConstants.JustificationMaxLength)
            return ServiceResult.Fail($"Justification cannot exceed {AppConstants.JustificationMaxLength} characters.");

        // No duplicate heads
        var dup = form.Lines.GroupBy(l => l.HeadId).Any(g => g.Count() > 1);
        if (dup) return ServiceResult.Fail("Duplicate expenditure head is not allowed.");

        var section = await _db.Sections.Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.SectionId == form.SectionId);
        if (section?.Department?.HasRevenue != "Y")
            return ServiceResult.Fail("Revenue entry is allowed only for DIT (HAS_REVENUE=Y).");

        var allotment = await _db.SectionFyRevenueAllotments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.SectionId == form.SectionId && a.FinancialYear == form.FinancialYear && a.IsActive == "Y");
        if (allotment == null)
            return ServiceResult.Fail("Revenue FY allotment not found for this section.");

        // blank → 0
        foreach (var line in form.Lines)
            if (line.Amount < 0) line.Amount = 0;

        var total = form.Lines.Sum(l => l.Amount);
        if (total > allotment.TotalAllotted)
            return ServiceResult.Fail(AppConstants.OverBudgetMessage);

        var exists = await _db.RevenueMonthlyEntries.AnyAsync(e =>
            e.SectionId == form.SectionId &&
            e.FinancialYear == form.FinancialYear &&
            e.EntryMonth == form.EntryMonth &&
            e.IsActive == "Y");
        if (exists)
            return ServiceResult.Fail("An entry already exists for this section, FY and month (write-lock).");

        var entry = new RevenueMonthlyEntry
        {
            SectionId = form.SectionId,
            FinancialYear = form.FinancialYear,
            EntryMonth = form.EntryMonth,
            JustificationText = form.JustificationText,
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
        return ServiceResult.Ok("Revenue entry submitted for checker approval.");
    }

    public async Task<List<RevenueSubmissionListItem>> GetSubmissionsAsync(string? makerPfFilter, long? deptIdFilter)
    {
        var q = from e in _db.RevenueMonthlyEntries.AsNoTracking()
                join s in _db.Sections.AsNoTracking() on e.SectionId equals s.SectionId
                where e.IsActive == "Y"
                select new { e, s };

        if (!string.IsNullOrWhiteSpace(makerPfFilter))
            q = q.Where(x => x.e.SubmittedByPf == makerPfFilter);
        if (deptIdFilter.HasValue)
            q = q.Where(x => x.s.DeptId == deptIdFilter.Value);

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
                FinancialYear = row.e.FinancialYear,
                Month = row.e.EntryMonth,
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
        var deptIds = await _db.Departments.AsNoTracking()
            .Where(d => d.CheckerPf == checkerPf && d.IsActive == "Y")
            .Select(d => d.DeptId)
            .ToListAsync();

        var q = from e in _db.RevenueMonthlyEntries.AsNoTracking()
                join s in _db.Sections.AsNoTracking() on e.SectionId equals s.SectionId
                where e.IsActive == "Y"
                      && e.EntryStatus == AppConstants.EntryStatus.Pending
                      && deptIds.Contains(s.DeptId)
                orderby e.SubmittedAt descending
                select new { e, s };

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
                FinancialYear = row.e.FinancialYear,
                Month = row.e.EntryMonth,
                SectionName = row.s.SectionName,
                TotalAmount = total,
                Status = row.e.EntryStatus,
                SubmittedByPf = row.e.SubmittedByPf
            });
        }
        return result;
    }

    public async Task<ServiceResult> CheckerActionAsync(long entryId, string action, string checkerPf, string? remark)
    {
        var entry = await _db.RevenueMonthlyEntries.FirstOrDefaultAsync(e => e.EntryId == entryId && e.IsActive == "Y");
        if (entry == null) return ServiceResult.Fail("Entry not found.");
        if (entry.EntryStatus != AppConstants.EntryStatus.Pending)
            return ServiceResult.Fail("Only PENDING entries can be actioned.");

        var section = await _db.Sections.FirstOrDefaultAsync(s => s.SectionId == entry.SectionId);
        var dept = section == null ? null :
            await _db.Departments.FirstOrDefaultAsync(d => d.DeptId == section.DeptId);
        if (dept == null ||
            (!string.Equals(dept.CheckerPf, checkerPf, StringComparison.OrdinalIgnoreCase)
             && !await IsAdminAsync(checkerPf)))
            return ServiceResult.Fail("You are not the Checker for this department.");

        var normalized = action.Trim().ToUpperInvariant();
        if (normalized is not (AppConstants.EntryStatus.Approved or AppConstants.EntryStatus.Rejected or AppConstants.EntryStatus.Returned))
            return ServiceResult.Fail("Invalid checker action.");

        entry.EntryStatus = normalized;
        entry.CheckedAt = DateTime.Now;
        entry.CheckedByPf = checkerPf;
        entry.CheckerRemark = remark;
        entry.UpdatedAt = DateTime.Now;
        entry.UpdatedBy = checkerPf;
        await _db.SaveChangesAsync();
        return ServiceResult.Ok($"Entry marked {normalized}.");
    }

    private async Task<bool> IsAdminAsync(string pf) =>
        await _db.AppUsers.AsNoTracking()
            .AnyAsync(u => u.PfNo == pf && u.RoleCode == AppConstants.Roles.Admin && u.IsActive == "Y");
}
