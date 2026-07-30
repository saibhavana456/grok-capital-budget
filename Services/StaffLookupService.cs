using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

/// <summary>
/// Staff lookup for Admin Maker/Checker View:
/// 1) SQL Server OrganisationsDb when configured (optional)
/// 2) Else Oracle app schema STAFF_DETAILS (EMPLID) — table created in SQL Developer
/// Empty sample data → null ("PF not found"). That is not a connection-config error.
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

    /// <summary>True when SQL Server Orgs is set, or when we can try Oracle STAFF_DETAILS.</summary>
    public bool IsOrganisationsConfigured => true;

    public async Task<StaffLookupResult?> LookupByPfAsync(string pfNo)
    {
        var pf = (pfNo ?? "").Trim();
        if (string.IsNullOrEmpty(pf))
            return null;

        var fromOrgs = await TryLookupSqlServerAsync(pf);
        if (fromOrgs != null) return fromOrgs;

        return await TryLookupOracleAsync(pf);
    }

    private async Task<StaffLookupResult?> TryLookupSqlServerAsync(string pf)
    {
        var conn = _config.GetConnectionString("OrganisationsDb");
        if (string.IsNullOrWhiteSpace(conn))
            return null;

        try
        {
            if (conn.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                conn = EncryptoData.DecryptAes(conn["ENC:".Length..]);

            var options = new DbContextOptionsBuilder<OrganisationsDbContext>()
                .UseSqlServer(conn)
                .Options;

            await using var orgDb = new OrganisationsDbContext(options);
            var staff = await orgDb.StaffDetails.AsNoTracking()
                .FirstOrDefaultAsync(s => s.EmplId == pf);
            return staff == null ? null : Map(staff, pf);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SQL Server STAFF_DETAILS lookup failed for {Pf}", pf);
            return null;
        }
    }

    private async Task<StaffLookupResult?> TryLookupOracleAsync(string pf)
    {
        try
        {
            await using var db = await _dbFactory.CreateDbContextAsync();
            // Table created in Oracle as STAFF_DETAILS with EMPLID (bank screenshot).
            var rows = await db.Set<OrgStaffDetail>()
                .FromSqlRaw(
                    """
                    SELECT EMPLID, NAME, LOCATION, DESCR, DEPTID, DESCR1,
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
                _logger.LogInformation("No Oracle STAFF_DETAILS row for PF {Pf}", pf);
                return null;
            }

            return Map(staff, pf);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Oracle STAFF_DETAILS lookup failed for {Pf}. Ensure table exists and sample data is loaded.",
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
