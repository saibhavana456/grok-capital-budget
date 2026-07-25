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

    public MasterService(AppDbContext db) => _db = db;

    public Task<List<Department>> GetDepartmentsAsync(bool activeOnly = true) =>
        _db.Departments.AsNoTracking()
            .Where(d => !activeOnly || d.IsActive == "Y")
            .OrderBy(d => d.DeptName)
            .ToListAsync();

    public Task<List<Department>> GetDepartmentsForMakerAsync(string makerPf)
    {
        var pf = (makerPf ?? "").Trim();
        return _db.Departments.AsNoTracking()
            .Where(d => d.IsActive == "Y" && d.MakerPf == pf)
            .OrderBy(d => d.DeptName)
            .ToListAsync();
    }

    public Task<List<Section>> GetSectionsByDeptAsync(long deptId, bool activeOnly = true) =>
        _db.Sections.AsNoTracking()
            .Where(s => s.DeptId == deptId && (!activeOnly || s.IsActive == "Y"))
            .OrderBy(s => s.SectionName)
            .ToListAsync();

    public Task<List<Project>> GetProjectsBySectionAsync(long sectionId, bool activeOnly = true) =>
        _db.Projects.AsNoTracking()
            .Where(p => p.SectionId == sectionId && (!activeOnly || p.IsActive == "Y"))
            .OrderBy(p => p.ProjectName)
            .ToListAsync();

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

    public async Task<ServiceResult> SaveDepartmentAsync(Department dept, string actorPf)
    {
        if (string.IsNullOrWhiteSpace(dept.DeptCode))
            return ServiceResult.Fail("Department code is required.");
        if (string.IsNullOrWhiteSpace(dept.DeptName))
            return ServiceResult.Fail("Department name is required.");

        var makerPf = dept.MakerPf?.Trim() ?? "";
        var checkerPf = dept.CheckerPf?.Trim() ?? "";
        dept.MakerPf = string.IsNullOrWhiteSpace(makerPf) ? null : makerPf;
        dept.CheckerPf = string.IsNullOrWhiteSpace(checkerPf) ? null : checkerPf;

        // Active department needs both Maker and Checker (Maker-Checker workflow)
        if (string.IsNullOrEmpty(makerPf))
            return ServiceResult.Fail(AppConstants.MakerPfRequiredMessage);
        if (string.IsNullOrEmpty(checkerPf))
            return ServiceResult.Fail(AppConstants.CheckerPfRequiredMessage);

        if (string.Equals(makerPf, checkerPf, StringComparison.OrdinalIgnoreCase))
            return ServiceResult.Fail(AppConstants.MakerCheckerSamePfMessage);

        // Collect all validation issues — keep Admin form open with values on failure
        var errors = new List<string>();

        var makerUser = await FindActiveAppUserAsync(makerPf);
        var checkerUser = await FindActiveAppUserAsync(checkerPf);

        if (makerUser == null)
            errors.Add(string.Format(AppConstants.MakerNotInAppUserMessage, makerPf));
        else if (string.Equals(makerUser.RoleCode, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            errors.Add(string.Format(AppConstants.AdminCannotBeMakerOrCheckerMessage, makerPf));
        else if (!string.Equals(makerUser.RoleCode, AppConstants.Roles.Maker, StringComparison.OrdinalIgnoreCase))
            errors.Add(string.Format(AppConstants.MakerNotInAppUserMessage, makerPf));

        if (checkerUser == null)
            errors.Add(string.Format(AppConstants.CheckerNotInAppUserMessage, checkerPf));
        else if (string.Equals(checkerUser.RoleCode, AppConstants.Roles.Admin, StringComparison.OrdinalIgnoreCase))
            errors.Add(string.Format(AppConstants.AdminCannotBeMakerOrCheckerMessage, checkerPf));
        else if (!string.Equals(checkerUser.RoleCode, AppConstants.Roles.Checker, StringComparison.OrdinalIgnoreCase))
            errors.Add(string.Format(AppConstants.CheckerNotInAppUserMessage, checkerPf));

        // One person → one active department (Maker or Checker role)
        var makerConflict = await FindActiveDeptPfConflictAsync(makerPf, dept.DeptId);
        if (makerConflict != null)
        {
            errors.Add(string.Format(
                AppConstants.PfAlreadyOnOtherDeptMessage,
                makerPf, makerConflict.Value.Role, makerConflict.Value.DeptName));
        }

        var checkerConflict = await FindActiveDeptPfConflictAsync(checkerPf, dept.DeptId);
        if (checkerConflict != null)
        {
            errors.Add(string.Format(
                AppConstants.PfAlreadyOnOtherDeptMessage,
                checkerPf, checkerConflict.Value.Role, checkerConflict.Value.DeptName));
        }

        if (errors.Count > 0)
            return ServiceResult.Fail(string.Join(" ", errors));

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
            // Do not overwrite IsActive here — soft-delete uses SoftDeleteDepartmentAsync.
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
        }
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Department saved.");
    }

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

    /// <summary>
    /// Returns conflicting role+dept name if PF is already Maker or Checker on another active department.
    /// </summary>
    private async Task<(string Role, string DeptName)?> FindActiveDeptPfConflictAsync(string pf, long excludeDeptId)
    {
        var pfNorm = pf.Trim();
        var rows = await _db.Departments.AsNoTracking()
            .Where(d => d.IsActive == "Y" && d.DeptId != excludeDeptId)
            .Select(d => new { d.DeptName, d.MakerPf, d.CheckerPf })
            .ToListAsync();

        foreach (var d in rows)
        {
            if (!string.IsNullOrWhiteSpace(d.MakerPf)
                && string.Equals(d.MakerPf.Trim(), pfNorm, StringComparison.OrdinalIgnoreCase))
                return ("Maker", d.DeptName);
            if (!string.IsNullOrWhiteSpace(d.CheckerPf)
                && string.Equals(d.CheckerPf.Trim(), pfNorm, StringComparison.OrdinalIgnoreCase))
                return ("Checker", d.DeptName);
        }

        return null;
    }

    public async Task<ServiceResult> SaveSectionAsync(Section section, string actorPf)
    {
        if (string.IsNullOrWhiteSpace(section.SectionName))
            return ServiceResult.Fail("Section name is required.");

        if (section.SectionId == 0)
        {
            section.CreatedAt = DateTime.Now;
            section.CreatedBy = actorPf;
            section.IsActive = "Y";
            _db.Sections.Add(section);
        }
        else
        {
            var existing = await _db.Sections.FirstOrDefaultAsync(s => s.SectionId == section.SectionId);
            if (existing == null) return ServiceResult.Fail("Section not found.");
            existing.SectionCode = section.SectionCode;
            existing.SectionName = section.SectionName;
            existing.Description = section.Description;
            existing.DeptId = section.DeptId;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
        }
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Section saved.");
    }

    public async Task<ServiceResult> SaveProjectAsync(Project project, string actorPf)
    {
        if (string.IsNullOrWhiteSpace(project.ProjectName))
            return ServiceResult.Fail("Project name is required.");

        if (project.ProjectId == 0)
        {
            project.CreatedAt = DateTime.Now;
            project.CreatedBy = actorPf;
            project.IsActive = "Y";
            _db.Projects.Add(project);
        }
        else
        {
            var existing = await _db.Projects.FirstOrDefaultAsync(p => p.ProjectId == project.ProjectId);
            if (existing == null) return ServiceResult.Fail("Project not found.");
            existing.ProjectCode = project.ProjectCode;
            existing.ProjectName = project.ProjectName;
            existing.SectionId = project.SectionId;
            existing.UpdatedAt = DateTime.Now;
            existing.UpdatedBy = actorPf;
        }
        await _db.SaveChangesAsync();
        return ServiceResult.Ok("Project saved.");
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
