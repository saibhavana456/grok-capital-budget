using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

/// <summary>
/// Organisations lookup via dbo.StaffDetails only (EMPLID, scale, email, phone).
/// Confirmed with Priyadarshini: scale eligibility and contact fields live on this table.
/// </summary>
public class StaffLookupService : IStaffLookupService
{
    private readonly IConfiguration _config;
    private readonly ILogger<StaffLookupService> _logger;

    public StaffLookupService(IConfiguration config, ILogger<StaffLookupService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool IsOrganisationsConfigured
    {
        get
        {
            var conn = _config.GetConnectionString("OrganisationsDb");
            return !string.IsNullOrWhiteSpace(conn);
        }
    }

    public async Task<StaffLookupResult?> LookupByPfAsync(string pfNo)
    {
        var conn = _config.GetConnectionString("OrganisationsDb");
        if (string.IsNullOrWhiteSpace(conn))
            return null;

        var pf = (pfNo ?? "").Trim();
        if (string.IsNullOrEmpty(pf))
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

            if (staff == null)
            {
                _logger.LogInformation("No Organisations StaffDetails row for PF {Pf}", pf);
                return null;
            }

            var scale = AppConstants.TryParseEmployeeScale(staff.EmpScaleCode, staff.EmpScaleDescr);
            return new StaffLookupResult
            {
                EmpId = staff.EmplId.Trim(),
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Organisations StaffDetails lookup failed for {Pf}", pfNo);
            return null;
        }
    }
}
