using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

/// <summary>
/// Admin unlock for a month after its deadline (end of next calendar month).
/// Capital: ProjectId set. Revenue: SectionId set.
/// </summary>
[Table("ENTRY_MONTH_UNLOCK")]
public class EntryMonthUnlock
{
    [Key]
    [Column("UNLOCK_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long UnlockId { get; set; }

    [Column("PROJECT_ID")]
    public long? ProjectId { get; set; }

    [Column("SECTION_ID")]
    public long? SectionId { get; set; }

    [Column("FINANCIAL_YEAR")]
    [Required, StringLength(20)]
    public string FinancialYear { get; set; } = string.Empty;

    [Column("ENTRY_MONTH")]
    [Required, StringLength(20)]
    public string EntryMonth { get; set; } = string.Empty;

    [Column("IS_ENABLED")]
    [StringLength(1)]
    public string IsEnabled { get; set; } = "Y";

    [Column("ENABLED_BY")]
    [StringLength(20)]
    public string? EnabledBy { get; set; }

    [Column("ENABLED_AT")]
    public DateTime EnabledAt { get; set; }

    [Column("UPDATED_AT")]
    public DateTime? UpdatedAt { get; set; }
}
