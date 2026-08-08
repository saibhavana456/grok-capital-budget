using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("PROJECT_FY_ALLOTMENT")]
public class ProjectFyAllotment
{
    [Key]
    [Column("ALLOTMENT_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long AllotmentId { get; set; }

    [Column("PROJECT_ID")]
    public long ProjectId { get; set; }

    [Column("FINANCIAL_YEAR")]
    [Required, StringLength(20)]
    public string FinancialYear { get; set; } = string.Empty;

    [Column("SPILLOVER_ALLOTTED", TypeName = "NUMBER(20,2)")]
    public decimal SpilloverAllotted { get; set; }

    [Column("FRESH_ALLOTTED", TypeName = "NUMBER(20,2)")]
    public decimal FreshAllotted { get; set; }

    [Column("TOTAL_ALLOTTED", TypeName = "NUMBER(20,2)")]
    public decimal TotalAllotted { get; set; }

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
