using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("REVENUE_MONTHLY_ENTRY")]
public class RevenueMonthlyEntry
{
    [Key]
    [Column("ENTRY_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long EntryId { get; set; }

    [Column("SECTION_ID")]
    public long SectionId { get; set; }

    [Column("FINANCIAL_YEAR")]
    [Required, StringLength(20)]
    public string FinancialYear { get; set; } = string.Empty;

    [Column("ENTRY_MONTH")]
    [Required, StringLength(20)]
    public string EntryMonth { get; set; } = string.Empty;

    [Column("JUSTIFICATION_TEXT")]
    public string? JustificationText { get; set; }

    [Column("ENTRY_STATUS")]
    [Required, StringLength(20)]
    public string EntryStatus { get; set; } = "PENDING";

    [Column("SUBMITTED_AT")]
    public DateTime SubmittedAt { get; set; }

    [Column("SUBMITTED_BY_PF")]
    [StringLength(20)]
    public string? SubmittedByPf { get; set; }

    [Column("CHECKED_AT")]
    public DateTime? CheckedAt { get; set; }

    [Column("CHECKED_BY_PF")]
    [StringLength(20)]
    public string? CheckedByPf { get; set; }

    [Column("CHECKER_REMARK")]
    [StringLength(2000)]
    public string? CheckerRemark { get; set; }

    [Column("IS_ACTIVE")]
    [StringLength(1)]
    public string IsActive { get; set; } = "Y";

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; }

    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }

    [Column("CREATED_BY")]
    [StringLength(100)]
    public string? CreatedBy { get; set; }

    [Column("UPDATED_BY")]
    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    [ForeignKey(nameof(SectionId))]
    public Section? Section { get; set; }

    public ICollection<RevenueMonthlyEntryLine> Lines { get; set; } = new List<RevenueMonthlyEntryLine>();
}
