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

        var code = dept.DeptCode.Trim();
        var name = dept.DeptName.Trim();
        dept.DeptCode = code;
        dept.DeptName = name;

        var isDit = AppConstants.IsDitDepartment(code);

        // Only one DIT; New Department is for non-DIT capital-only depts.
        if (isDit)
        {
            var ditExists = (await _db.Departments.AsNoTracking()
                    .Where(d => d.IsActive == "Y" && d.DeptId != dept.DeptId)
                    .Select(d => d.DeptCode)
                    .ToListAsync())
                .Any(c => string.Equals(c, AppConstants.DitDeptCode, StringComparison.OrdinalIgnoreCase));
            if (ditExists || dept.DeptId == 0)
                return ServiceResult.Fail(AppConstants.DitAlreadyExistsMessage);

            dept.HasRevenue = "Y";
            dept.MakerPf = null;
            dept.CheckerPf = null;
        }
        else
        {
            dept.HasRevenue = "N";
            var makerPf = dept.MakerPf?.Trim() ?? "";
            var checkerPf = dept.CheckerPf?.Trim() ?? "";
            dept.MakerPf = string.IsNullOrWhiteSpace(makerPf) ? null : makerPf;
            dept.CheckerPf = string.IsNullOrWhiteSpace(checkerPf) ? null : checkerPf;

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

        var others = await _db.Departments.AsNoTracking()
            .Where(d => d.IsActive == "Y" && d.DeptId != dept.DeptId)
            .Select(d => new { d.DeptCode, d.DeptName })
            .ToListAsync();
        if (others.Any(d => string.Equals(d.DeptCode, code, StringComparison.OrdinalIgnoreCase)))
            return ServiceResult.Fail(string.Format(AppConstants.DuplicateDeptCodeMessage, code));
        if (others.Any(d => string.Equals(d.DeptName, name, StringComparison.OrdinalIgnoreCase)))
            return ServiceResult.Fail(string.Format(AppConstants.DuplicateDeptNameMessage, name));

        if (dept.DeptId == 0)
        {
            if (isDit)
                return ServiceResult.Fail(AppConstants.DitAlreadyExistsMessage);

            dept.CreatedAt = DateTime.Now;
            dept.CreatedBy = actorPf;
            dept.IsActive = "Y";
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();

            // Non-DIT capital-only: auto default section so Admin can add Projects without Sections tab.
            await EnsureDefaultCapitalSectionAsync(dept.DeptId, name, actorPf);
        }
        else
        {
            var existing = await _db.Departments.FirstOrDefaultAsync(d => d.DeptId == dept.DeptId);
            if (existing == null) return ServiceResult.Fail("Department not found.");

            if (AppConstants.IsDitDepartment(existing.DeptCode))
            {
                // Existing DIT: keep code DIT; allow name update only; never dept M/C.
                existing.DeptName = name;
                existing.HasRevenue = "Y";
                existing.MakerPf = null;
                existing.CheckerPf = null;
            }
            else
            {
                existing.DeptCode = code;
                existing.DeptName = name;
                existing.HasRevenue = "N";
                existing.MakerPf = dept.MakerPf;
                existing.CheckerPf = dept.CheckerPf;
                await EnsureDefaultCapitalSectionAsync(existing.DeptId, name, actorPf);
            }

            // Allow Admin to reactivate (Y) or keep inactive (N) from Edit — soft-delete only sets N.
            existing.IsActive = NormalizeYn(dept.IsActive);
            if (existing.IsActive == "Y" && AppConstants.IsDitDepartment(existing.DeptCode))
            {
                var otherActiveDit = (await _db.Departments.AsNoTracking()
                        .Where(d => d.IsActive == "Y" && d.DeptId != existing.DeptId)
                        .Select(d => d.DeptCode)
                        .ToListAsync())
                    .Any(c => string.Equals(c, AppConstants.DitDeptCode, StringComparison.OrdinalIgnoreCase));
                if (otherActiveDit)
                    return ServiceResult.Fail(AppConstants.DitAlreadyExistsMessage);
            }

            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
            await _db.SaveChangesAsync();
        }

        return ServiceResult.Ok("Department saved.");
    }

    private static string NormalizeYn(string? value) =>
        string.Equals(value?.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ? "Y" : "N";

    /// <summary>
    /// Non-DIT departments have no real Sections UX — create/reuse GENERAL section for projects.
    /// </summary>
    public async Task<Section> EnsureDefaultCapitalSectionAsync(long deptId, string? deptName, string actorPf)
    {
        var existing = await _db.Sections
            .FirstOrDefaultAsync(s => s.DeptId == deptId
                                      && s.IsActive == "Y"
                                      && (s.SectionCode == AppConstants.DefaultCapitalSectionCode
                                          || s.SectionName == AppConstants.DefaultCapitalSectionName));
        if (existing != null) return existing;

        var any = await _db.Sections.FirstOrDefaultAsync(s => s.DeptId == deptId && s.IsActive == "Y");
        if (any != null) return any;

        var section = new Section
        {
            DeptId = deptId,
            SectionCode = AppConstants.DefaultCapitalSectionCode,
            SectionName = string.IsNullOrWhiteSpace(deptName)
                ? AppConstants.DefaultCapitalSectionName
                : $"{deptName.Trim()} — {AppConstants.DefaultCapitalSectionName}",
            MakerPf = null,
            CheckerPf = null,
            IsActive = "Y",
            CreatedAt = DateTime.Now,
            CreatedBy = actorPf
        };
        _db.Sections.Add(section);
        await _db.SaveChangesAsync();
        return section;
    }

    public async Task<ServiceResult> SaveSectionAsync(
        Section section, string actorPf, decimal? revenueAllotted = null, string? financialYear = null,
        bool forRevenue = false)
    {
        if (string.IsNullOrWhiteSpace(section.SectionName))
            return ServiceResult.Fail("Section name is required.");

        var dept = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DeptId == section.DeptId);
        if (dept == null) return ServiceResult.Fail("Department not found.");

        var makerPf = section.MakerPf?.Trim() ?? "";
        var checkerPf = section.CheckerPf?.Trim() ?? "";
        section.MakerPf = string.IsNullOrWhiteSpace(makerPf) ? null : makerPf;
        section.CheckerPf = string.IsNullOrWhiteSpace(checkerPf) ? null : checkerPf;

        // DIT Revenue: section-wise Maker/Checker (Priyadarshini). Capital sections: no section M/C.
        if (forRevenue && AppConstants.IsDitDepartment(dept.DeptCode) && dept.HasRevenue == "Y")
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
            existing.IsActive = NormalizeYn(section.IsActive);
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
            await _db.SaveChangesAsync();
            section.SectionId = existing.SectionId;
        }

        if (revenueAllotted.HasValue && forRevenue && dept.HasRevenue == "Y")
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
            existing.IsActive = NormalizeYn(project.IsActive);
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

    public async Task<bool> IsMonthUnlockedAsync(
        long? projectId, long? sectionId, string financialYear, string entryMonth)
    {
        var fy = (financialYear ?? "").Trim();
        var month = (entryMonth ?? "").Trim();
        if (string.IsNullOrEmpty(fy) || string.IsNullOrEmpty(month)) return false;

        try
        {
            if (projectId.HasValue && projectId.Value > 0)
            {
                return await _db.EntryMonthUnlocks.AsNoTracking()
                    .AnyAsync(u => u.ProjectId == projectId.Value
                                   && u.FinancialYear == fy
                                   && u.EntryMonth == month
                                   && u.IsEnabled == "Y");
            }

            if (sectionId.HasValue && sectionId.Value > 0)
            {
                return await _db.EntryMonthUnlocks.AsNoTracking()
                    .AnyAsync(u => u.SectionId == sectionId.Value
                                   && u.FinancialYear == fy
                                   && u.EntryMonth == month
                                   && u.IsEnabled == "Y");
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    public async Task<ServiceResult> EnableMonthUnlockAsync(
        long? projectId, long? sectionId, string financialYear, string entryMonth, string actorPf)
    {
        var fy = (financialYear ?? "").Trim();
        var month = (entryMonth ?? "").Trim();
        if (string.IsNullOrEmpty(fy) || string.IsNullOrEmpty(month))
            return ServiceResult.Fail("Financial year and month are required.");

        var isCapital = projectId.HasValue && projectId.Value > 0;
        var isRevenue = sectionId.HasValue && sectionId.Value > 0;
        if (isCapital == isRevenue)
            return ServiceResult.Fail("Provide either ProjectId (capital) or SectionId (revenue), not both/neither.");

        try
        {
            // Raw SQL avoids Oracle IDENTITY insert issues with EF tracked entities.
            if (isCapital)
            {
                var updated = await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE ENTRY_MONTH_UNLOCK
SET IS_ENABLED = 'Y', ENABLED_BY = {actorPf}, ENABLED_AT = SYSTIMESTAMP, UPDATED_AT = SYSTIMESTAMP
WHERE PROJECT_ID = {projectId!.Value}
  AND FINANCIAL_YEAR = {fy}
  AND ENTRY_MONTH = {month}");

                if (updated == 0)
                {
                    await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO ENTRY_MONTH_UNLOCK
  (PROJECT_ID, SECTION_ID, FINANCIAL_YEAR, ENTRY_MONTH, IS_ENABLED, ENABLED_BY, ENABLED_AT)
VALUES
  ({projectId.Value}, NULL, {fy}, {month}, 'Y', {actorPf}, SYSTIMESTAMP)");
                }
            }
            else
            {
                var updated = await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE ENTRY_MONTH_UNLOCK
SET IS_ENABLED = 'Y', ENABLED_BY = {actorPf}, ENABLED_AT = SYSTIMESTAMP, UPDATED_AT = SYSTIMESTAMP
WHERE SECTION_ID = {sectionId!.Value}
  AND FINANCIAL_YEAR = {fy}
  AND ENTRY_MONTH = {month}");

                if (updated == 0)
                {
                    await _db.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO ENTRY_MONTH_UNLOCK
  (PROJECT_ID, SECTION_ID, FINANCIAL_YEAR, ENTRY_MONTH, IS_ENABLED, ENABLED_BY, ENABLED_AT)
VALUES
  (NULL, {sectionId.Value}, {fy}, {month}, 'Y', {actorPf}, SYSTIMESTAMP)");
                }
            }

            var ok = await IsMonthUnlockedAsync(projectId, sectionId, fy, month);
            if (!ok)
                return ServiceResult.Fail(
                    "Unlock insert did not persist. Confirm ENTRY_MONTH_UNLOCK exists and app Oracle user can INSERT.");

            return ServiceResult.Ok(
                isCapital
                    ? $"Enabled {month} for capital project {projectId} (FY {fy})."
                    : $"Enabled {month} for revenue section {sectionId} (FY {fy}).");
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail($"Unlock failed: {ex.Message}");
        }
    }
}
