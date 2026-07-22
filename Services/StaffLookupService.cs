using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

public class StaffLookupService : IStaffLookupService
{
    private readonly IConfiguration _config;
    private readonly ILogger<StaffLookupService> _logger;

    public StaffLookupService(IConfiguration config, ILogger<StaffLookupService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<StaffLookupResult?> LookupByPfAsync(string pfNo)
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
                .FirstOrDefaultAsync(s => s.EmpId == pfNo);
            if (staff == null) return null;
            return new StaffLookupResult
            {
                EmpId = staff.EmpId,
                EmpName = staff.EmpName ?? pfNo,
                Designation = staff.EmpDesgnDesc
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "STAFF_DETAILS lookup failed for {Pf}", pfNo);
            return null;
        }
    }
}
