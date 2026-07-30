using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Data;

/// <summary>
/// SQL Server Organisations DB connection (set OrganisationsDb in appsettings).
/// Maps the staff master table the bank created as <c>STAFF_DETAILS</c> with
/// EMPLID / NAME / EMP_SCALE_CODE / EMAIL / PHONE (same shape as former StaffDetails).
/// Empty OrganisationsDb connection string → lookup skipped (not an empty-data error).
/// </summary>
public class OrganisationsDbContext : DbContext
{
    public OrganisationsDbContext(DbContextOptions<OrganisationsDbContext> options) : base(options)
    {
    }

    public DbSet<OrgStaffDetail> StaffDetails => Set<OrgStaffDetail>();
}

/// <summary>
/// Bank staff master — table name STAFF_DETAILS, key EMPLID (as created in SQL Developer).
/// </summary>
[Table("STAFF_DETAILS")]
[Keyless]
public class OrgStaffDetail
{
    [Column("EMPLID")]
    public string EmplId { get; set; } = string.Empty;

    [Column("NAME")]
    public string? Name { get; set; }

    [Column("LOCATION")]
    public string? Location { get; set; }

    [Column("DESCR")]
    public string? LocationDesc { get; set; }

    [Column("DEPTID")]
    public string? DeptId { get; set; }

    [Column("DESCR1")]
    public string? DeptDesc { get; set; }

    [Column("EMP_DESGN")]
    public string? EmpDesgn { get; set; }

    [Column("EMP_DESGN_DESC")]
    public string? EmpDesgnDesc { get; set; }

    [Column("EMP_SCALE_CODE")]
    public string? EmpScaleCode { get; set; }

    [Column("EMP_SCALE_DESCR")]
    public string? EmpScaleDescr { get; set; }

    [Column("PHONE")]
    public string? Phone { get; set; }

    [Column("EMAIL")]
    public string? Email { get; set; }
}
