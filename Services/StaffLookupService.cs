using IT_BUDGET_MONITORING_PORTAL.Data;
using IT_BUDGET_MONITORING_PORTAL.Helpers;
using IT_BUDGET_MONITORING_PORTAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Services;

/// <summary>
/// Organisations integration matching Personal/SCV:
/// 1) Look up PF in STAFF_DETAILS (EMP_ID) for name / designation / location.
/// 2) Enrich EMAIL / PHONE from StaffDetails (EMPLID) when that row exists.
/// Returns null when OrganisationsDb connection is empty (local/dev without Orgs).
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

            // SCV Admin: FirstOrDefault on STAFF_DETAILS where EMP_ID == pf
            var staff = await orgDb.StaffDetailsMaster.AsNoTracking()
                .FirstOrDefaultAsync(s => s.EmpId == pf);

            // Contact table (EMAIL/PHONE) — may exist even if STAFF_DETAILS row missing
            StaffDetailsContact? contact = null;
            try
            {
                contact = await orgDb.StaffDetailsContacts.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.EmplId == pf);
            }
            catch (Exception contactEx)
            {
                // Table may not be granted yet — do not fail identity lookup
                _logger.LogWarning(contactEx, "StaffDetails (contact) lookup failed for {Pf}", pf);
            }

            if (staff == null && contact == null)
            {
                _logger.LogInformation("No Organisations staff row for PF {Pf}", pf);
                return null;
            }

            return new StaffLookupResult
            {
                EmpId = staff?.EmpId?.Trim() ?? contact!.EmplId.Trim(),
                EmpName = FirstNonEmpty(staff?.EmpName, contact?.Name, pf),
                Designation = FirstNonEmpty(staff?.EmpDesgnDesc, contact?.EmpDesgnDesc),
                DesignationCode = staff?.EmpDesgn,
                LocationCode = FirstNonEmpty(staff?.Location, contact?.Location),
                LocationDesc = FirstNonEmpty(staff?.LocationDesc, contact?.LocationDesc),
                DeptDesc = FirstNonEmpty(staff?.DeptIdDesc, contact?.DeptDesc),
                Email = contact?.Email,
                Phone = contact?.Phone,
                HasContactFromStaffDetails = contact != null
                    && (!string.IsNullOrWhiteSpace(contact.Email) || !string.IsNullOrWhiteSpace(contact.Phone))
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Organisations staff lookup failed for {Pf}", pfNo);
            return null;
        }
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var v in values)
            if (!string.IsNullOrWhiteSpace(v))
                return v.Trim();
        return string.Empty;
    }
}
