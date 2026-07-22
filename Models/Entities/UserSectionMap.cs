using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("USER_SECTION_MAP")]
public class UserSectionMap
{
    [Key]
    [Column("MAP_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long MapId { get; set; }

    [Column("USER_ID")]
    public long UserId { get; set; }

    [Column("SECTION_ID")]
    public long SectionId { get; set; }

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
}
