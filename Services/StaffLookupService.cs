using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

/// <summary>
/// Staff lookup by EMPLID:
/// 1) When Auth:UseOrganisationsDb=true → SQL Server Organisations <c>StaffDetails</c> (SCV ConnStrOrganisations)
/// 2) Else → Oracle app schema <c>STAFF_DETAILS</c> (same EMPLID columns for local testing)
/// Organisations tables are read-only — no DDL against org DB.
/// </summary>
public class StaffLookupService : IStaffLookupService
{
    private readonly IConfiguration _config;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<StaffLookupService> _logger;

    public StaffLookupService(
        IConfiguration config,
        IDbContextFactory<AppDbContext> dbFactory,
        ILogger<StaffLookupService> logger)
    {
        _config = config;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    /// <summary>Staff source is always available (Oracle mirror and/or Organisations).</summary>
    public bool IsOrganisationsConfigured => true;

    public bool UseOrganisationsDb => _config.GetValue("Auth:UseOrganisationsDb", false);

    public async Task<StaffLookupResult?> LookupByPfAsync(string pfNo)
    {
        var pf = (pfNo ?? "").Trim();
        if (string.IsNullOrEmpty(pf))
            return null;

        if (UseOrganisationsDb)
        {
            var fromOrgs = await TryLookupSqlServerStaffDetailsAsync(pf);
            if (fromOrgs != null) return fromOrgs;
            _logger.LogInformation(
                "UseOrganisationsDb=true but no StaffDetails row for EMPLID {Pf}", pf);
            return null;
        }

        return await TryLookupOracleAsync(pf);
    }

    private async Task<StaffLookupResult?> TryLookupSqlServerStaffDetailsAsync(string pf)
    {
        var raw = _config.GetConnectionString("OrganisationsDb");
        var conn = ConnectionStringHelper.Resolve(raw);
        if (string.IsNullOrWhiteSpace(conn))
        {
            _logger.LogWarning(
                "Auth:UseOrganisationsDb=true but ConnectionStrings:OrganisationsDb is empty.");
            return null;
        }

        try
        {
            var options = new DbContextOptionsBuilder<OrganisationsDbContext>()
                .UseSqlServer(conn)
                .Options;

            await using var orgDb = new OrganisationsDbContext(options);
            // Organisations dbo.StaffDetails — EMPLID (same shape as Oracle STAFF_DETAILS mirror)
            var staff = await orgDb.StaffDetails.AsNoTracking()
                .FirstOrDefaultAsync(s => s.EmplId == pf);
            return staff == null ? null : Map(staff, pf);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SQL Server StaffDetails lookup failed for {Pf}", pf);
            return null;
        }
    }

    private async Task<StaffLookupResult?> TryLookupOracleAsync(string pf)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            // Oracle STAFF_DETAILS — EMPLID columns matching Organisations StaffDetails
            var rows = await db.Set<OrgStaffDetail>()
                .FromSqlRaw(
                    """
                    SELECT EMPLID, NAME, LOCATION, DESCR, DEPTID, DESCR1,
                           REGION_CODE, REGION_NAME, DIVISION_CODE, DIVISION_NAME,
                           EMP_DESGN, EMP_DESGN_DESC, EMP_SCALE_CODE, EMP_SCALE_DESCR,
                           PHONE, EMAIL
                    FROM STAFF_DETAILS
                    WHERE EMPLID = {0}
                    """, pf)
                .AsNoTracking()
                .ToListAsync();

            var staff = rows.FirstOrDefault();
            if (staff == null)
            {
                _logger.LogInformation("No Oracle STAFF_DETAILS row for EMPLID {Pf}", pf);
                return null;
            }

            return Map(staff, pf);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Oracle STAFF_DETAILS lookup failed for {Pf}. Run Scripts/CREATE_ORACLE_STAFF_DETAILS.sql then SEED_STAFF_DETAILS_MAKER_CHECKER.sql.",
                pf);
            return null;
        }
    }

    private static StaffLookupResult Map(OrgStaffDetail staff, string pf)
    {
        var scale = AppConstants.TryParseEmployeeScale(staff.EmpScaleCode, staff.EmpScaleDescr);
        return new StaffLookupResult
        {
            EmpId = (staff.EmplId ?? pf).Trim(),
            EmpName = string.IsNullOrWhiteSpace(staff.Name) ? pf : staff.Name.Trim(),
            Designation = staff.EmpDesgnDesc,
            DesignationCode = staff.EmpDesgn,
            LocationCode = staff.Location,
            LocationDesc = staff.LocationDesc,
            DeptDesc = staff.DeptDesc,
            Email = staff.Email,
            Phone = staff.Phone,
            ScaleCode = staff.EmpScaleCode,
            ScaleDescr = staff.EmpScaleDescr,
            ScaleNumber = scale,
            HasContactFromStaffDetails =
                !string.IsNullOrWhiteSpace(staff.Email) || !string.IsNullOrWhiteSpace(staff.Phone)
        };
    }
}
