using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("CAPITAL_MONTHLY_ENTRY")]
public class CapitalMonthlyEntry
{
    [Key]
    [Column("ENTRY_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long EntryId { get; set; }

    [Column("PROJECT_ID")]
    public long ProjectId { get; set; }

    [Column("FINANCIAL_YEAR")]
    [Required, StringLength(20)]
    public string FinancialYear { get; set; } = string.Empty;

    [Column("ENTRY_MONTH")]
    [Required, StringLength(20)]
    public string EntryMonth { get; set; } = string.Empty;

    [Column("ACTUAL_SPILLOVER", TypeName = "NUMBER(20,2)")]
    public decimal ActualSpillover { get; set; }

    [Column("ACTUAL_FRESH", TypeName = "NUMBER(20,2)")]
    public decimal ActualFresh { get; set; }

    [Column("ACTUAL_TOTAL", TypeName = "NUMBER(20,2)")]
    public decimal ActualTotal { get; set; }

    [Column("EST_SPILLOVER_NEXT", TypeName = "NUMBER(20,2)")]
    public decimal EstSpilloverNext { get; set; }

    [Column("EST_FRESH_NEXT", TypeName = "NUMBER(20,2)")]
    public decimal EstFreshNext { get; set; }

    [Column("EST_TOTAL_NEXT", TypeName = "NUMBER(20,2)")]
    public decimal EstTotalNext { get; set; }

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

    [ForeignKey(nameof(ProjectId))]
    public Project? Project { get; set; }
}
