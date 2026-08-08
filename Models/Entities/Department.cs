using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("DEPARTMENT")]
public class Department
{
    [Key]
    [Column("DEPT_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long DeptId { get; set; }

    [Column("DEPT_CODE")]
    [StringLength(50)]
    public string? DeptCode { get; set; }

    [Column("DEPT_NAME")]
    [Required, StringLength(200)]
    public string DeptName { get; set; } = string.Empty;

    [Column("HAS_REVENUE")]
    [StringLength(1)]
    public string HasRevenue { get; set; } = "N";

    [Column("MAKER_PF")]
    [StringLength(20)]
    public string? MakerPf { get; set; }

    [Column("CHECKER_PF")]
    [StringLength(20)]
    public string? CheckerPf { get; set; }

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

    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
