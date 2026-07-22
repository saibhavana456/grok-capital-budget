using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Data;

/// <summary>
/// SQL Server Organisations DB — STAFF_DETAILS only (Personal/SCV pattern).
/// Captcha lives in Oracle LOGIN_CAPTCHA_QUESTION for this app.
/// </summary>
public class OrganisationsDbContext : DbContext
{
    public OrganisationsDbContext(DbContextOptions<OrganisationsDbContext> options) : base(options)
    {
    }

    public DbSet<StaffDetail> StaffDetails => Set<StaffDetail>();
}

[Table("STAFF_DETAILS")]
public class StaffDetail
{
    [Key]
    [Column("EMP_ID")]
    [StringLength(50)]
    public string EmpId { get; set; } = string.Empty;

    [Column("EMP_NAME")]
    [StringLength(200)]
    public string? EmpName { get; set; }

    [Column("EMP_DESGN_DESC")]
    [StringLength(200)]
    public string? EmpDesgnDesc { get; set; }

    [Column("DEPTID")]
    [StringLength(50)]
    public string? DeptId { get; set; }

    [Column("LOCATION_DESC")]
    [StringLength(200)]
    public string? LocationDesc { get; set; }
}
