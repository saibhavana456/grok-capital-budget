using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using IT_BUDGET_MONITORING_PORTAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

public class MasterService : IMasterService
{
    private readonly AppDbContext _db;
    private readonly IStaffLookupService _staffLookup;

    public MasterService(AppDbContext db, IStaffLookupService staffLookup)
    {
        _db = db;
        _staffLookup = staffLookup;
    }

    public Task<List<Department>> GetDepartmentsAsync(bool activeOnly = true) =>
        _db.Departments.AsNoTracking()
            .Where(d => !activeOnly || d.IsActive == "Y")
            .OrderBy(d => d.DeptName)
            .ToListAsync();

    public async Task<List<Department>> GetDepartmentsForMakerAsync(string makerPf)
    {
        var pf = (makerPf ?? "").Trim();
        if (string.IsNullOrEmpty(pf)) return new List<Department>();

        var depts = await _db.Departments.AsNoTracking()
            .Where(d => d.IsActive == "Y")
            .ToListAsync();

        var projectDeptIds = await (
            from p in _db.Projects.AsNoTracking()
            join s in _db.Sections.AsNoTracking() on p.SectionId equals s.SectionId
            where p.IsActive == "Y" && s.IsActive == "Y" && p.MakerPf != null
            select new { s.DeptId, p.MakerPf }
        ).ToListAsync();

        var sectionDeptIds = await _db.Sections.AsNoTracking()
            .Where(s => s.IsActive == "Y" && s.MakerPf != null)
            .Select(s => new { s.DeptId, s.MakerPf })
            .ToListAsync();

        return depts.Where(d =>
            {
                if (PfEquals(d.MakerPf, pf)) return true;
                if (AppConstants.IsDitDepartment(d.DeptCode))
                {
                    if (projectDeptIds.Any(x => x.DeptId == d.DeptId && PfEquals(x.MakerPf, pf)))
                        return true;
                    if (sectionDeptIds.Any(x => x.DeptId == d.DeptId && PfEquals(x.MakerPf, pf)))
                        return true;
                }
                return false;
            })
            .OrderBy(d => d.DeptName)
            .ToList();
    }

    public Task<List<Section>> GetSectionsByDeptAsync(long deptId, bool activeOnly = true) =>
        _db.Sections.AsNoTracking()
            .Where(s => s.DeptId == deptId && (!activeOnly || s.IsActive == "Y"))
            .OrderBy(s => s.SectionName)
            .ToListAsync();

    public async Task<List<Section>> GetSectionsForMakerAsync(
        long deptId, string makerPf, bool capitalPath, bool activeOnly = true)
    {
        var dept = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DeptId == deptId);
        var sections = await GetSectionsByDeptAsync(deptId, activeOnly);
        if (dept == null || !AppConstants.IsDitDepartment(dept.DeptCode))
            return sections;

        var pf = (makerPf ?? "").Trim();
        if (capitalPath)
        {
            var sectionIds = await _db.Projects.AsNoTracking()
                .Where(p => p.IsActive == "Y" && sections.Select(s => s.SectionId).Contains(p.SectionId))
                .Where(p => p.MakerPf != null)
                .ToListAsync();
            var allowed = sectionIds
                .Where(p => PfEquals(p.MakerPf, pf))
                .Select(p => p.SectionId)
                .ToHashSet();
            // Non-DIT-style fallback: if dept maker matches, all sections
            if (PfEquals(dept.MakerPf, pf)) return sections;
            return sections.Where(s => allowed.Contains(s.SectionId)).ToList();
        }

        if (PfEquals(dept.MakerPf, pf)) return sections;
        return sections.Where(s => PfEquals(s.MakerPf, pf)).ToList();
    }

    public Task<List<Project>> GetProjectsBySectionAsync(long sectionId, bool activeOnly = true) =>
        _db.Projects.AsNoTracking()
            .Where(p => p.SectionId == sectionId && (!activeOnly || p.IsActive == "Y"))
            .OrderBy(p => p.ProjectName)
            .ToListAsync();

    public async Task<List<Project>> GetProjectsForMakerAsync(long sectionId, string makerPf, bool activeOnly = true)
    {
        var projects = await GetProjectsBySectionAsync(sectionId, activeOnly);
        var section = await _db.Sections.Include(s => s.Department).AsNoTracking()
            .FirstOrDefaultAsync(s => s.SectionId == sectionId);
        if (section?.Department == null || !AppConstants.IsDitDepartment(section.Department.DeptCode))
            return projects;

        var pf = (makerPf ?? "").Trim();
        if (PfEquals(section.Department.MakerPf, pf))
            return projects;
        return projects.Where(p => PfEquals(p.MakerPf, pf)).ToList();
    }

    public Task<List<RevenueHead>> GetRevenueHeadsAsync() =>
        _db.RevenueHeads.AsNoTracking()
            .Where(h => h.IsActive == "Y")
            .OrderBy(h => h.DisplayOrder)
            .ToListAsync();

    public Task<Department?> GetDepartmentAsync(long deptId) =>
        _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DeptId == deptId);

    public Task<Section?> GetSectionAsync(long sectionId) =>
        _db.Sections.Include(s => s.Department).AsNoTracking()
            .FirstOrDefaultAsync(s => s.SectionId == sectionId);

    public Task<Project?> GetProjectAsync(long projectId) =>
        _db.Projects.Include(p => p.Section)!.ThenInclude(s => s!.Department)
            .AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId);

    public Task<ProjectFyAllotment?> GetProjectAllotmentAsync(long projectId, string financialYear) =>
        _db.ProjectFyAllotments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.ProjectId == projectId
                                      && a.FinancialYear == financialYear
                                      && a.IsActive == "Y");

    public Task<SectionFyRevenueAllotment?> GetSectionRevenueAllotmentAsync(long sectionId, string financialYear) =>
        _db.SectionFyRevenueAllotments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.SectionId == sectionId
                                      && a.FinancialYear == financialYear
                                      && a.IsActive == "Y");

    public async Task<ServiceResult> SaveDepartmentAsync(Department dept, string actorPf)
    {
        if (string.IsNullOrWhiteSpace(dept.DeptCode))
            return ServiceResult.Fail("Department code is required.");
        if (string.IsNullOrWhiteSpace(dept.DeptName))
            return ServiceResult.Fail("Department name is required.");

        var isDit = AppConstants.IsDitDepartment(dept.DeptCode);
        var makerPf = dept.MakerPf?.Trim() ?? "";
        var checkerPf = dept.CheckerPf?.Trim() ?? "";
        dept.MakerPf = string.IsNullOrWhiteSpace(makerPf) ? null : makerPf;
        dept.CheckerPf = string.IsNullOrWhiteSpace(checkerPf) ? null : checkerPf;

        // DIT: Maker/Checker live on project (capital) / section (revenue) — clear dept fields.
        if (isDit)
        {
            dept.MakerPf = null;
            dept.CheckerPf = null;
        }
        else
        {
            if (string.IsNullOrEmpty(makerPf))
                return ServiceResult.Fail(AppConstants.MakerPfRequiredMessage);
            if (string.IsNullOrEmpty(checkerPf))
                return ServiceResult.Fail(AppConstants.CheckerPfRequiredMessage);
            if (string.Equals(makerPf, checkerPf, StringComparison.OrdinalIgnoreCase))
                return ServiceResult.Fail(AppConstants.MakerCheckerSamePfMessage);

            var assignErrors = await ValidateMakerCheckerPairAsync(makerPf, checkerPf);
            if (assignErrors.Count > 0)
                return ServiceResult.Fail(string.Join(" ", assignErrors));
        }

        if (dept.DeptId == 0)
        {
            dept.CreatedAt = DateTime.Now;
            dept.CreatedBy = actorPf;
            dept.IsActive = "Y";
            _db.Departments.Add(dept);
        }
        else
        {
            var existing = await _db.Departments.FirstOrDefaultAsync(d => d.DeptId == dept.DeptId);
            if (existing == null) return ServiceResult.Fail("Department not found.");
            existing.DeptCode = dept.DeptCode;
            existing.DeptName = dept.DeptName;
            existing.HasRevenue = dept.HasRevenue;
            existing.MakerPf = dept.MakerPf;
            existing.CheckerPf = dept.CheckerPf;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
        }
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Department saved.");
    }

    public async Task<ServiceResult> SaveSectionAsync(
        Section section, string actorPf, decimal? revenueAllotted = null, string? financialYear = null)
    {
        if (string.IsNullOrWhiteSpace(section.SectionName))
            return ServiceResult.Fail("Section name is required.");

        var dept = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DeptId == section.DeptId);
        if (dept == null) return ServiceResult.Fail("Department not found.");

        var makerPf = section.MakerPf?.Trim() ?? "";
        var checkerPf = section.CheckerPf?.Trim() ?? "";
        section.MakerPf = string.IsNullOrWhiteSpace(makerPf) ? null : makerPf;
        section.CheckerPf = string.IsNullOrWhiteSpace(checkerPf) ? null : checkerPf;

        // Revenue (DIT HasRevenue=Y): section-wise Maker/Checker. Otherwise clear — capital uses project/dept.
        if (AppConstants.IsDitDepartment(dept.DeptCode) && dept.HasRevenue == "Y")
        {
            if (string.IsNullOrEmpty(makerPf) || string.IsNullOrEmpty(checkerPf))
                return ServiceResult.Fail("DIT revenue section requires Maker PF and Checker PF.");
            if (string.Equals(makerPf, checkerPf, StringComparison.OrdinalIgnoreCase))
                return ServiceResult.Fail(AppConstants.MakerCheckerSamePfMessage);
            var assignErrors = await ValidateMakerCheckerPairAsync(makerPf, checkerPf);
            if (assignErrors.Count > 0)
                return ServiceResult.Fail(string.Join(" ", assignErrors));
        }
        else
        {
            section.MakerPf = null;
            section.CheckerPf = null;
        }

        if (section.SectionId == 0)
        {
            section.CreatedAt = DateTime.Now;
            section.CreatedBy = actorPf;
            section.IsActive = "Y";
            _db.Sections.Add(section);
            await _db.SaveChangesAsync();
        }
        else
        {
            var existing = await _db.Sections.FirstOrDefaultAsync(s => s.SectionId == section.SectionId);
            if (existing == null) return ServiceResult.Fail("Section not found.");
            existing.SectionCode = section.SectionCode;
            existing.SectionName = section.SectionName;
            existing.Description = section.Description;
            existing.DeptId = section.DeptId;
            existing.MakerPf = section.MakerPf;
            existing.CheckerPf = section.CheckerPf;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
            await _db.SaveChangesAsync();
            section.SectionId = existing.SectionId;
        }

        if (revenueAllotted.HasValue && dept.HasRevenue == "Y")
        {
            var fy = string.IsNullOrWhiteSpace(financialYear)
                ? AppConstants.CurrentFinancialYear()
                : financialYear.Trim();
            await UpsertSectionRevenueAllotmentAsync(section.SectionId, fy, Math.Max(0, revenueAllotted.Value), actorPf);
        }

        return ServiceResult.Ok("Section saved.");
    }

    public async Task<ServiceResult> SaveProjectAsync(
        Project project, string actorPf, decimal? spillover = null, decimal? fresh = null, string? financialYear = null)
    {
        if (string.IsNullOrWhiteSpace(project.ProjectName))
            return ServiceResult.Fail("Project name is required.");

        var section = await _db.Sections.Include(s => s.Department).AsNoTracking()
            .FirstOrDefaultAsync(s => s.SectionId == project.SectionId);
        if (section?.Department == null) return ServiceResult.Fail("Section not found.");

        var makerPf = project.MakerPf?.Trim() ?? "";
        var checkerPf = project.CheckerPf?.Trim() ?? "";
        project.MakerPf = string.IsNullOrWhiteSpace(makerPf) ? null : makerPf;
        project.CheckerPf = string.IsNullOrWhiteSpace(checkerPf) ? null : checkerPf;

        // DIT capital: project-wise Maker/Checker. Non-DIT: clear — use department Maker/Checker.
        if (AppConstants.IsDitDepartment(section.Department.DeptCode))
        {
            if (string.IsNullOrEmpty(makerPf) || string.IsNullOrEmpty(checkerPf))
                return ServiceResult.Fail("DIT capital project requires Maker PF and Checker PF.");
            if (string.Equals(makerPf, checkerPf, StringComparison.OrdinalIgnoreCase))
                return ServiceResult.Fail(AppConstants.MakerCheckerSamePfMessage);
            var assignErrors = await ValidateMakerCheckerPairAsync(makerPf, checkerPf);
            if (assignErrors.Count > 0)
                return ServiceResult.Fail(string.Join(" ", assignErrors));
        }
        else
        {
            project.MakerPf = null;
            project.CheckerPf = null;
        }

        if (project.ProjectId == 0)
        {
            project.CreatedAt = DateTime.Now;
            project.CreatedBy = actorPf;
            project.IsActive = "Y";
            _db.Projects.Add(project);
            await _db.SaveChangesAsync();
        }
        else
        {
            var existing = await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);
            if (existing == null) return ServiceResult.Fail("Project not found.");
            existing.ProjectCode = project.ProjectCode;
            existing.ProjectName = project.ProjectName;
            existing.SectionId = project.SectionId;
            existing.MakerPf = project.MakerPf;
            existing.CheckerPf = project.CheckerPf;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
            await _db.SaveChangesAsync();
            project.ProjectId = existing.ProjectId;
        }

        if (spillover.HasValue || fresh.HasValue)
        {
            var fy = string.IsNullOrWhiteSpace(financialYear)
                ? AppConstants.CurrentFinancialYear()
                : financialYear.Trim();
            var sp = Math.Max(0, spillover ?? 0);
            var fr = Math.Max(0, fresh ?? 0);
            await UpsertProjectAllotmentAsync(project.ProjectId, fy, sp, fr, actorPf);
        }

        return ServiceResult.Ok("Project saved.");
    }

    private async Task UpsertProjectAllotmentAsync(
        long projectId, string fy, decimal spillover, decimal fresh, string actorPf)
    {
        var row = await _db.ProjectFyAllotments
            .FirstOrDefaultAsync(a => a.ProjectId == projectId && a.FinancialYear == fy && a.IsActive == "Y");
        if (row == null)
        {
            _db.ProjectFyAllotments.Add(new ProjectFyAllotment
            {
                ProjectId = projectId,
                FinancialYear = fy,
                SpilloverAllotted = spillover,
                FreshAllotted = fresh,
                TotalAllotted = spillover + fresh,
                IsActive = "Y",
                CreatedAt = DateTime.Now,
                CreatedBy = actorPf
            });
        }
        else
        {
            row.SpilloverAllotted = spillover;
            row.FreshAllotted = fresh;
            row.TotalAllotted = spillover + fresh;
            row.UpdatedAt = DateTime.Now;
            row.UpdatedBy = actorPf;
        }
        await _db.SaveChangesAsync();
    }

    private async Task UpsertSectionRevenueAllotmentAsync(
        long sectionId, string fy, decimal total, string actorPf)
    {
        var row = await _db.SectionFyRevenueAllotments
            .FirstOrDefaultAsync(a => a.SectionId == sectionId && a.FinancialYear == fy && a.IsActive == "Y");
        if (row == null)
        {
            _db.SectionFyRevenueAllotments.Add(new SectionFyRevenueAllotment
            {
                SectionId = sectionId,
                FinancialYear = fy,
                TotalAllotted = total,
                IsActive = "Y",
                CreatedAt = DateTime.Now,
                CreatedBy = actorPf
            });
        }
        else
        {
            row.TotalAllotted = total;
            row.UpdatedAt = DateTime.Now;
            row.UpdatedBy = actorPf;
        }
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// APP_USER must exist (MAKER or CHECKER, not ADMIN).
    /// When Organisations configured: StaffDetails required + scale 1–4 maker / 4+ checker.
    /// Same PF may be Maker on one project and Checker on another (scale 4) — no global uniqueness.
    /// </summary>
    private async Task<List<string>> ValidateMakerCheckerPairAsync(string makerPf, string checkerPf)
    {
        var errors = new List<string>();
        var makerUser = await FindActiveAppUserAsync(makerPf);
        var checkerUser = await FindActiveAppUserAsync(checkerPf);

        if (makerUser == null)
            errors.Add(string.Format(AppConstants.MakerNotInAppUserMessage, makerPf));
        else if (string.Equals(makerUser.RoleCode, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            errors.Add(string.Format(AppConstants.AdminCannotBeMakerOrCheckerMessage, makerPf));
        else if (!IsMakerOrCheckerRole(makerUser.RoleCode))
            errors.Add(string.Format(AppConstants.MakerNotInAppUserMessage, makerPf));

        if (checkerUser == null)
            errors.Add(string.Format(AppConstants.CheckerNotInAppUserMessage, checkerPf));
        else if (string.Equals(checkerUser.RoleCode, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            errors.Add(string.Format(AppConstants.AdminCannotBeMakerOrCheckerMessage, checkerPf));
        else if (!IsMakerOrCheckerRole(checkerUser.RoleCode))
            errors.Add(string.Format(AppConstants.CheckerNotInAppUserMessage, checkerPf));

        if (_staffLookup.IsOrganisationsConfigured)
        {
            var makerStaff = await _staffLookup.LookupByPfAsync(makerPf);
            if (makerStaff == null)
                errors.Add(string.Format(AppConstants.StaffNotInOrganisationsMessage, makerPf));
            else if (!AppConstants.IsMakerScaleAllowed(makerStaff.ScaleNumber))
                errors.Add(string.Format(AppConstants.MakerScaleInvalidMessage, makerPf,
                    FormatScale(makerStaff)));

            var checkerStaff = await _staffLookup.LookupByPfAsync(checkerPf);
            if (checkerStaff == null)
                errors.Add(string.Format(AppConstants.StaffNotInOrganisationsMessage, checkerPf));
            else if (!AppConstants.IsCheckerScaleAllowed(checkerStaff.ScaleNumber))
                errors.Add(string.Format(AppConstants.CheckerScaleInvalidMessage, checkerPf,
                    FormatScale(checkerStaff)));
        }

        return errors;
    }

    private static bool IsMakerOrCheckerRole(string? role) =>
        string.Equals(role, AppConstants.Roles.Maker, StringComparison.OrdinalIgnoreCase)
        || string.Equals(role, AppConstants.Roles.Checker, StringComparison.OrdinalIgnoreCase);

    private static string FormatScale(StaffLookupResult s) =>
        s.ScaleNumber?.ToString()
        ?? (string.IsNullOrWhiteSpace(s.ScaleCode) ? (s.ScaleDescr ?? "unknown") : s.ScaleCode);

    private static bool PfEquals(string? a, string b) =>
        !string.IsNullOrWhiteSpace(a)
        && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private async Task<AppUser?> FindActiveAppUserAsync(string pf)
    {
        var pfNorm = pf.Trim();
        var users = await _db.AppUsers.AsNoTracking()
            .Where(u => u.IsActive == "Y")
            .Select(u => new AppUser { PfNo = u.PfNo, RoleCode = u.RoleCode, UserId = u.UserId })
            .ToListAsync();
        return users.FirstOrDefault(u =>
            string.Equals(u.PfNo.Trim(), pfNorm, StringComparison.OrdinalIgnoreCase));
    }

    public async Task SoftDeleteDepartmentAsync(long deptId, string actorPf)
    {
        var d = await _db.Departments.FirstOrDefaultAsync(x => x.DeptId == deptId);
        if (d == null) return;
        d.IsActive = "N";
        d.UpdatedAt = DateTime.Now;
        d.UpdatedBy = actorPf;
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteSectionAsync(long sectionId, string actorPf)
    {
        var s = await _db.Sections.FirstOrDefaultAsync(x => x.SectionId == sectionId);
        if (s == null) return;
        s.IsActive = "N";
        s.UpdatedAt = DateTime.Now;
        s.UpdatedBy = actorPf;
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteProjectAsync(long projectId, string actorPf)
    {
        var p = await _db.Projects.FirstOrDefaultAsync(x => x.ProjectId == projectId);
        if (p == null) return;
        p.IsActive = "N";
        p.UpdatedAt = DateTime.Now;
        p.UpdatedBy = actorPf;
        await _db.SaveChangesAsync();
    }
}
