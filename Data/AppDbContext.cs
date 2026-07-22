using IT_BUDGET_MONITORING_PORTAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IT_BUDGET_MONITORING_PORTAL.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<DepartmentAuthority> DepartmentAuthorities => Set<DepartmentAuthority>();
    public DbSet<SectionAuthority> SectionAuthorities => Set<SectionAuthority>();
    public DbSet<SectionTeamMember> SectionTeamMembers => Set<SectionTeamMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectAssignment> ProjectAssignments => Set<ProjectAssignment>();
    public DbSet<ProjectFyAllotment> ProjectFyAllotments => Set<ProjectFyAllotment>();
    public DbSet<CapitalMonthlyEntry> CapitalMonthlyEntries => Set<CapitalMonthlyEntry>();
    public DbSet<RevenueHead> RevenueHeads => Set<RevenueHead>();
    public DbSet<SectionFyRevenueAllotment> SectionFyRevenueAllotments => Set<SectionFyRevenueAllotment>();
    public DbSet<RevenueMonthlyEntry> RevenueMonthlyEntries => Set<RevenueMonthlyEntry>();
    public DbSet<RevenueMonthlyEntryLine> RevenueMonthlyEntryLines => Set<RevenueMonthlyEntryLine>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<UserSectionMap> UserSectionMaps => Set<UserSectionMap>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<LoginCaptchaQuestion> LoginCaptchaQuestions => Set<LoginCaptchaQuestion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CapitalMonthlyEntry>()
            .HasIndex(e => new { e.ProjectId, e.FinancialYear, e.EntryMonth })
            .IsUnique();

        modelBuilder.Entity<RevenueMonthlyEntry>()
            .HasIndex(e => new { e.SectionId, e.FinancialYear, e.EntryMonth })
            .IsUnique();

        modelBuilder.Entity<RevenueMonthlyEntryLine>()
            .HasIndex(e => new { e.EntryId, e.HeadId })
            .IsUnique();

        modelBuilder.Entity<ProjectFyAllotment>()
            .HasIndex(e => new { e.ProjectId, e.FinancialYear })
            .IsUnique();

        modelBuilder.Entity<SectionFyRevenueAllotment>()
            .HasIndex(e => new { e.SectionId, e.FinancialYear })
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .HasIndex(e => e.PfNo)
            .IsUnique();
    }
}
