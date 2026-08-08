using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Data;

/// <summary>
/// SQL Server Organisations DB (SCV <c>ConnStrOrganisations</c>).
/// Maps Organisations <c>StaffDetails</c> (EMPLID) — the latest staff master used for scale/email/phone.
/// Do not alter Organisations tables; read existing columns only.
/// Local testing uses Oracle app-schema <c>STAFF_DETAILS</c> with the same EMPLID column shape.
/// </summary>
public class OrganisationsDbContext : DbContext
{
    public OrganisationsDbContext(DbContextOptions<OrganisationsDbContext> options) : base(options)
    {
    }

    public DbSet<OrgStaffDetail> StaffDetails => Set<OrgStaffDetail>();
}

/// <summary>
/// Organisations dbo.StaffDetails (and Oracle STAFF_DETAILS mirror) — EMPLID key.
/// Column list matches Personal/SCV Organisations StaffDetails DDL (script.sql).
/// </summary>
[Table("StaffDetails")]
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

    [Column("REGION_CODE")]
    public string? RegionCode { get; set; }

    [Column("REGION_NAME")]
    public string? RegionName { get; set; }

    [Column("DIVISION_CODE")]
    public string? DivisionCode { get; set; }

    [Column("DIVISION_NAME")]
    public string? DivisionName { get; set; }

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
