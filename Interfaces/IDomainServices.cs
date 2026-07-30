using IT_BUDGET_MONITORING_PORTAL.Models.DTOs;
using IT_BUDGET_MONITORING_PORTAL.Models.Entities;

namespace IT_BUDGET_MONITORING_PORTAL.Interfaces;

public interface IMasterService
{
    Task<List<Department>> GetDepartmentsAsync(bool activeOnly = true);
    /// <summary>Active departments where this PF is the department Maker (one Maker → their dept only).</summary>
    Task<List<Department>> GetDepartmentsForMakerAsync(string makerPf);
    Task<List<Section>> GetSectionsByDeptAsync(long deptId, bool activeOnly = true);
    Task<List<Project>> GetProjectsBySectionAsync(long sectionId, bool activeOnly = true);
    Task<List<RevenueHead>> GetRevenueHeadsAsync();
    Task<Department?> GetDepartmentAsync(long deptId);
    Task<Section?> GetSectionAsync(long sectionId);
    Task<Project?> GetProjectAsync(long projectId);
    Task<ServiceResult> SaveDepartmentAsync(Department dept, string actorPf);
    Task<ServiceResult> SaveSectionAsync(Section section, string actorPf);
    Task<ServiceResult> SaveProjectAsync(Project project, string actorPf);
    Task SoftDeleteDepartmentAsync(long deptId, string actorPf);
    Task SoftDeleteSectionAsync(long sectionId, string actorPf);
    Task SoftDeleteProjectAsync(long projectId, string actorPf);
}

public interface ICapitalService
{
    Task<CapitalEntryFormDto?> BuildFormAsync(long projectId, string financialYear, string entryMonth);
    Task<ExistingEntryInfo?> FindActiveEntryAsync(long projectId, string financialYear, string entryMonth);
    Task<ServiceResult> SubmitAsync(CapitalEntryFormDto form, string makerPf);
    Task<List<CapitalSubmissionListItem>> GetSubmissionsAsync(string? makerPfFilter, long? deptIdFilter, string? checkerPfFilter = null);
    Task<CapitalMonthlyEntry?> GetEntryAsync(long entryId);
    Task<ServiceResult> CheckerActionAsync(long entryId, string action, string checkerPf, string? remark);
    Task<List<CapitalSubmissionListItem>> GetPendingForCheckerAsync(string checkerPf);
}

public interface IRevenueService
{
    Task<RevenueEntryFormDto?> BuildFormAsync(long sectionId, string financialYear, string entryMonth);
    Task<ExistingEntryInfo?> FindActiveEntryAsync(long sectionId, string financialYear, string entryMonth);
    Task<ServiceResult> SubmitAsync(RevenueEntryFormDto form, string makerPf);
    Task<List<RevenueSubmissionListItem>> GetSubmissionsAsync(string? makerPfFilter, long? deptIdFilter, string? checkerPfFilter = null);
    Task<RevenueMonthlyEntry?> GetEntryAsync(long entryId);
    Task<ServiceResult> CheckerActionAsync(long entryId, string action, string checkerPf, string? remark);
    Task<List<RevenueSubmissionListItem>> GetPendingForCheckerAsync(string checkerPf);
}

public interface IStaffLookupService
{
    /// <summary>
    /// Organisations DB lookup (SCV pattern): STAFF_DETAILS for identity,
    /// StaffDetails (EMPLID) for EMAIL/PHONE when present.
    /// Returns null when OrganisationsDb is not configured or PF not found.
    /// </summary>
    Task<StaffLookupResult?> LookupByPfAsync(string pfNo);
    bool IsOrganisationsConfigured { get; }
}

public class StaffLookupResult
{
    public string EmpId { get; set; } = string.Empty;
    public string EmpName { get; set; } = string.Empty;
    public string? Designation { get; set; }
    public string? DesignationCode { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationDesc { get; set; }
    public string? DeptDesc { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    /// <summary>True when EMAIL/PHONE came from Organisations StaffDetails table.</summary>
    public bool HasContactFromStaffDetails { get; set; }
}
