using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Data;

/// <summary>
/// SQL Server Organisations DB — same pattern as Personal/SCV.
/// <list type="bullet">
/// <item><see cref="StaffDetail"/> → dbo.STAFF_DETAILS (EMP_ID identity / designation / location; no email/phone)</item>
/// <item><see cref="StaffDetailsContact"/> → dbo.StaffDetails (EMPLID + EMAIL + PHONE — the mixed-case table)</item>
/// </list>
/// Captcha for this app lives in Oracle LOGIN_CAPTCHA_QUESTION (not Organisations).
/// </summary>
public class OrganisationsDbContext : DbContext
{
    public OrganisationsDbContext(DbContextOptions<OrganisationsDbContext> options) : base(options)
    {
    }

    /// <summary>SCV primary staff master — table name STAFF_DETAILS.</summary>
    public DbSet<StaffDetail> StaffDetailsMaster => Set<StaffDetail>();

    /// <summary>SCV contact enrichment — table name StaffDetails (EMAIL, PHONE).</summary>
    public DbSet<StaffDetailsContact> StaffDetailsContacts => Set<StaffDetailsContact>();
}

/// <summary>Maps Organisations dbo.STAFF_DETAILS (uppercase) — SCV Admin GetStaffByEmpId source.</summary>
[Table("STAFF_DETAILS")]
[Keyless]
public class StaffDetail
{
    [Column("EMP_ID")]
    [StringLength(50)]
    public string EmpId { get; set; } = string.Empty;

    [Column("EMP_NAME")]
    [StringLength(200)]
    public string? EmpName { get; set; }

    [Column("LOCATION")]
    [StringLength(50)]
    public string? Location { get; set; }

    [Column("LOCATION_DESC")]
    [StringLength(200)]
    public string? LocationDesc { get; set; }

    [Column("DEPTID")]
    [StringLength(50)]
    public string? DeptId { get; set; }

    [Column("DEPT_ID_DESC")]
    [StringLength(200)]
    public string? DeptIdDesc { get; set; }

    [Column("EMP_DESGN")]
    [StringLength(50)]
    public string? EmpDesgn { get; set; }

    [Column("EMP_DESGN_DESC")]
    [StringLength(200)]
    public string? EmpDesgnDesc { get; set; }
}

/// <summary>
/// Maps Organisations dbo.StaffDetails (mixed-case) — has EMAIL and PHONE.
/// SCV uses this table for OTP phone; Priyadarshini admin wants contact fields from here.
/// Sample data to be loaded when provided — lookup is optional until OrganisationsDb is configured.
/// </summary>
[Table("StaffDetails")]
[Keyless]
public class StaffDetailsContact
{
    [Column("EMPLID")]
    [StringLength(50)]
    public string EmplId { get; set; } = string.Empty;

    [Column("NAME")]
    [StringLength(250)]
    public string? Name { get; set; }

    [Column("EMP_DESGN_DESC")]
    [StringLength(250)]
    public string? EmpDesgnDesc { get; set; }

    [Column("LOCATION")]
    [StringLength(250)]
    public string? Location { get; set; }

    [Column("DESCR")]
    [StringLength(250)]
    public string? LocationDesc { get; set; }

    [Column("DEPTID")]
    [StringLength(250)]
    public string? DeptId { get; set; }

    [Column("DESCR1")]
    [StringLength(250)]
    public string? DeptDesc { get; set; }

    [Column("PHONE")]
    [StringLength(250)]
    public string? Phone { get; set; }

    [Column("EMAIL")]
    [StringLength(250)]
    public string? Email { get; set; }
}
