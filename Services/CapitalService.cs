using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using IT_BUDGET_MONITORING_PORTAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

public class CapitalService : ICapitalService
{
    private readonly AppDbContext _db;

    public CapitalService(AppDbContext db) => _db = db;

    public async Task<CapitalEntryFormDto?> BuildFormAsync(long projectId, string financialYear, string entryMonth)
    {
        var project = await _db.Projects
            .Include(p => p.Section)!.ThenInclude(s => s!.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.IsActive == "Y");
        if (project?.Section == null) return null;

        var allotment = await _db.ProjectFyAllotments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ProjectId == projectId && a.FinancialYear == financialYear && a.IsActive == "Y");

        var prevMonth = AppConstants.PreviousMonth(entryMonth);
        var prev = await _db.CapitalMonthlyEntries.AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == projectId
                                      && e.FinancialYear == financialYear
                                      && e.EntryMonth == prevMonth
                                      && e.IsActive == "Y"
                                      && e.EntryStatus == AppConstants.EntryStatus.Approved);

        var utilizedTillPrev = await SumApprovedUtilizedThroughAsync(projectId, financialYear, prevMonth);

        var form = new CapitalEntryFormDto
        {
            ProjectId = projectId,
            FinancialYear = financialYear,
            EntryMonth = entryMonth,
            ProjectName = project.ProjectName,
            SectionName = project.Section.SectionName,
            DeptName = project.Section.Department?.DeptName ?? "",
            SpilloverAllotted = allotment?.SpilloverAllotted ?? 0,
            FreshAllotted = allotment?.FreshAllotted ?? 0,
            TotalAllotted = allotment?.TotalAllotted ?? 0,
            UtilizedTillPreviousMonth = utilizedTillPrev,
            PrevSpillover = prev?.ActualSpillover ?? 0,
            PrevFresh = prev?.ActualFresh ?? 0,
            PrevTotal = prev?.ActualTotal ?? 0
        };

        var editable = await _db.CapitalMonthlyEntries.AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProjectId == projectId
                                      && e.FinancialYear == financialYear
                                      && e.EntryMonth == entryMonth
                                      && e.IsActive == "Y"
                                      && (e.EntryStatus == AppConstants.EntryStatus.Returned
                                          || e.EntryStatus == AppConstants.EntryStatus.Rejected));
        if (editable != null)
        {
            form.ActualSpillover = editable.ActualSpillover;
            form.ActualFresh = editable.ActualFresh;
            form.EstSpilloverNext = editable.EstSpilloverNext;
            form.EstFreshNext = editable.EstFreshNext;
            form.JustificationText = editable.JustificationText ?? "";
        }

        return form;
    }

    public async Task<ExistingEntryInfo?> FindActiveEntryAsync(long projectId, string financialYear, string entryMonth)
    {
        return await _db.CapitalMonthlyEntries.AsNoTracking()
            .Where(e => e.ProjectId == projectId
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

    public async Task<ServiceResult> SubmitAsync(CapitalEntryFormDto form, string makerPf)
    {
        if (form.ProjectId <= 0) return ServiceResult.Fail("Project is required.");
        if (string.IsNullOrWhiteSpace(form.EntryMonth)) return ServiceResult.Fail("Month is required.");
        if (AppConstants.IsFutureMonth(form.EntryMonth))
            return ServiceResult.Fail("Future month entry is not allowed.");
        if (string.IsNullOrWhiteSpace(form.JustificationText))
            return ServiceResult.Fail(AppConstants.JustificationRequiredMessage);
        if (form.JustificationText.Length > AppConstants.JustificationMaxLength)
            return ServiceResult.Fail($"Justification cannot exceed {AppConstants.JustificationMaxLength} characters.");

        var allotment = await _db.ProjectFyAllotments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ProjectId == form.ProjectId && a.FinancialYear == form.FinancialYear && a.IsActive == "Y");
        if (allotment == null)
            return ServiceResult.Fail("FY allotment not found for this project.");

        // Normalize negatives; blank numeric fields bind as 0 (0 is allowed).
        form.ActualSpillover = Math.Max(0, form.ActualSpillover);
        form.ActualFresh = Math.Max(0, form.ActualFresh);
        form.EstSpilloverNext = Math.Max(0, form.EstSpilloverNext);
        form.EstFreshNext = Math.Max(0, form.EstFreshNext);

        var actualTotal = form.ActualSpillover + form.ActualFresh;

        var existing = await _db.CapitalMonthlyEntries
            .FirstOrDefaultAsync(e => e.ProjectId == form.ProjectId
                                      && e.FinancialYear == form.FinancialYear
                                      && e.EntryMonth == form.EntryMonth
                                      && e.IsActive == "Y");

        if (existing != null)
        {
            if (AppConstants.IsLockedStatus(existing.EntryStatus))
                return ServiceResult.Fail("An entry already exists for this project, FY and month.");

            if (!AppConstants.IsEditableStatus(existing.EntryStatus))
                return ServiceResult.Fail("This entry cannot be resubmitted.");

            existing.ActualSpillover = form.ActualSpillover;
            existing.ActualFresh = form.ActualFresh;
            existing.ActualTotal = actualTotal;
            existing.EstSpilloverNext = form.EstSpilloverNext;
            existing.EstFreshNext = form.EstFreshNext;
            existing.EstTotalNext = form.EstSpilloverNext + form.EstFreshNext;
            existing.JustificationText = form.JustificationText.Trim();
            existing.EntryStatus = AppConstants.EntryStatus.Pending;
            existing.SubmittedAt = DateTime.Now;
            existing.SubmittedByPf = makerPf;
            existing.CheckedAt = null;
            existing.CheckedByPf = null;
            existing.CheckerRemark = null;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = makerPf;
            await _db.SaveChangesAsync();
            return ServiceResult.Ok(AppConstants.SuccessResubmit);
        }

        if (!AppConstants.IsAllowedEntryMonth(form.EntryMonth))
            return ServiceResult.Fail("Entry is allowed only for the current month or the previous month.");

        var entry = new CapitalMonthlyEntry
        {
            ProjectId = form.ProjectId,
            FinancialYear = form.FinancialYear,
            EntryMonth = form.EntryMonth,
            ActualSpillover = form.ActualSpillover,
            ActualFresh = form.ActualFresh,
            ActualTotal = actualTotal,
            EstSpilloverNext = form.EstSpilloverNext,
            EstFreshNext = form.EstFreshNext,
            EstTotalNext = form.EstSpilloverNext + form.EstFreshNext,
            JustificationText = form.JustificationText.Trim(),
            EntryStatus = AppConstants.EntryStatus.Pending,
            SubmittedAt = DateTime.Now,
            SubmittedByPf = makerPf,
            IsActive = "Y",
            CreatedAt = DateTime.Now,
            CreatedBy = makerPf
        };
        _db.CapitalMonthlyEntries.Add(entry);
        await _db.SaveChangesAsync();
        return ServiceResult.Ok(AppConstants.SuccessSubmit);
    }

    public async Task<List<CapitalSubmissionListItem>> GetSubmissionsAsync(string? makerPfFilter, long? deptIdFilter)
    {
        var q = from e in _db.CapitalMonthlyEntries.AsNoTracking()
                join p in _db.Projects.AsNoTracking() on e.ProjectId equals p.ProjectId
                join s in _db.Sections.AsNoTracking() on p.SectionId equals s.SectionId
                where e.IsActive == "Y"
                select new { e, p, s };

        if (!string.IsNullOrWhiteSpace(makerPfFilter))
            q = q.Where(x => x.e.SubmittedByPf == makerPfFilter);
        if (deptIdFilter.HasValue)
            q = q.Where(x => x.s.DeptId == deptIdFilter.Value);

        return await q.OrderByDescending(x => x.e.SubmittedAt)
            .Select(x => new CapitalSubmissionListItem
            {
                EntryId = x.e.EntryId,
                FinancialYear = x.e.FinancialYear,
                Month = x.e.EntryMonth,
                SectionName = x.s.SectionName,
                ProjectName = x.p.ProjectName,
                Status = x.e.EntryStatus,
                SubmittedByPf = x.e.SubmittedByPf
            }).ToListAsync();
    }

    public Task<CapitalMonthlyEntry?> GetEntryAsync(long entryId) =>
        _db.CapitalMonthlyEntries
            .Include(e => e.Project)!.ThenInclude(p => p!.Section)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EntryId == entryId);

    public async Task<List<CapitalSubmissionListItem>> GetPendingForCheckerAsync(string checkerPf)
    {
        var deptIds = await _db.Departments.AsNoTracking()
            .Where(d => d.CheckerPf == checkerPf && d.IsActive == "Y")
            .Select(d => d.DeptId)
            .ToListAsync();

        var q = from e in _db.CapitalMonthlyEntries.AsNoTracking()
                join p in _db.Projects.AsNoTracking() on e.ProjectId equals p.ProjectId
                join s in _db.Sections.AsNoTracking() on p.SectionId equals s.SectionId
                where e.IsActive == "Y"
                      && e.EntryStatus == AppConstants.EntryStatus.Pending
                      && deptIds.Contains(s.DeptId)
                orderby e.SubmittedAt descending
                select new CapitalSubmissionListItem
                {
                    EntryId = e.EntryId,
                    FinancialYear = e.FinancialYear,
                    Month = e.EntryMonth,
                    SectionName = s.SectionName,
                    ProjectName = p.ProjectName,
                    Status = e.EntryStatus,
                    SubmittedByPf = e.SubmittedByPf
                };

        return await q.ToListAsync();
    }

    public async Task<ServiceResult> CheckerActionAsync(long entryId, string action, string checkerPf, string? remark)
    {
        var entry = await _db.CapitalMonthlyEntries.FirstOrDefaultAsync(e => e.EntryId == entryId && e.IsActive == "Y");
        if (entry == null) return ServiceResult.Fail("Entry not found.");
        if (entry.EntryStatus != AppConstants.EntryStatus.Pending)
            return ServiceResult.Fail("Only PENDING entries can be actioned.");

        var project = await _db.Projects.Include(p => p.Section).FirstOrDefaultAsync(p => p.ProjectId == entry.ProjectId);
        var dept = project?.Section == null ? null :
            await _db.Departments.FirstOrDefaultAsync(d => d.DeptId == project.Section.DeptId);
        if (dept == null ||
            !string.Equals(dept.CheckerPf, checkerPf, StringComparison.OrdinalIgnoreCase))
            return ServiceResult.Fail("You are not the Checker for this department.");

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

        return ServiceResult.Ok(normalized switch
        {
            AppConstants.EntryStatus.Approved => AppConstants.SuccessApprove,
            AppConstants.EntryStatus.Returned => AppConstants.SuccessReturn,
            _ => AppConstants.SuccessReject
        });
    }

    private async Task<decimal> SumApprovedUtilizedThroughAsync(long projectId, string financialYear, string throughMonth)
    {
        var months = AppConstants.MonthsFromAprilThrough(throughMonth).ToList();
        if (months.Count == 0) return 0;

        return await _db.CapitalMonthlyEntries.AsNoTracking()
            .Where(e => e.ProjectId == projectId
                        && e.FinancialYear == financialYear
                        && e.IsActive == "Y"
                        && e.EntryStatus == AppConstants.EntryStatus.Approved
                        && months.Contains(e.EntryMonth))
            .SumAsync(e => (decimal?)e.ActualTotal) ?? 0;
    }
}
