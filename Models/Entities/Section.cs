using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("SECTION")]
public class Section
{
    [Key]
    [Column("SECTION_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SectionId { get; set; }

    [Column("DEPT_ID")]
    public long DeptId { get; set; }

    [Column("SECTION_CODE")]
    [StringLength(50)]
    public string? SectionCode { get; set; }

    [Column("SECTION_NAME")]
    [Required, StringLength(300)]
    public string SectionName { get; set; } = string.Empty;

    [Column("DESCRIPTION")]
    [StringLength(500)]
    public string? Description { get; set; }

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

    [ForeignKey(nameof(DeptId))]
    public Department? Department { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
