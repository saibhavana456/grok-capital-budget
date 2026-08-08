using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("REVENUE_MONTHLY_ENTRY_LINE")]
public class RevenueMonthlyEntryLine
{
    [Key]
    [Column("LINE_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long LineId { get; set; }

    [Column("ENTRY_ID")]
    public long EntryId { get; set; }

    [Column("HEAD_ID")]
    public long HeadId { get; set; }

    [Column("AMOUNT", TypeName = "NUMBER(20,2)")]
    public decimal Amount { get; set; }

    [ForeignKey(nameof(EntryId))]
    public RevenueMonthlyEntry? Entry { get; set; }

    [ForeignKey(nameof(HeadId))]
    public RevenueHead? Head { get; set; }
}
