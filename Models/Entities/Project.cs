using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("PROJECT")]
public class Project
{
    [Key]
    [Column("PROJECT_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ProjectId { get; set; }

    [Column("SECTION_ID")]
    public long SectionId { get; set; }

    [Column("PROJECT_CODE")]
    [StringLength(50)]
    public string? ProjectCode { get; set; }

    [Column("PROJECT_NAME")]
    [Required, StringLength(500)]
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>DIT capital: Maker/Checker are project-wise (Priyadarshini 30-Jul-2026).</summary>
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

    [ForeignKey(nameof(SectionId))]
    public Section? Section { get; set; }
}
