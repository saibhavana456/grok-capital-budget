using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("REVENUE_HEAD")]
public class RevenueHead
{
    [Key]
    [Column("HEAD_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long HeadId { get; set; }

    [Column("HEAD_CODE")]
    [Required, StringLength(50)]
    public string HeadCode { get; set; } = string.Empty;

    [Column("HEAD_NAME")]
    [Required, StringLength(100)]
    public string HeadName { get; set; } = string.Empty;

    [Column("DISPLAY_ORDER")]
    public int DisplayOrder { get; set; }

    [Column("IS_ACTIVE")]
    [StringLength(1)]
    public string IsActive { get; set; } = "Y";
}
