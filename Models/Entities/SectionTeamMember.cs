using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("SECTION_TEAM_MEMBER")]
public class SectionTeamMember
{
    [Key]
    [Column("MEMBER_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long MemberId { get; set; }

    [Column("SECTION_ID")]
    public long SectionId { get; set; }

    [Column("PF_NO")]
    [Required, StringLength(20)]
    public string PfNo { get; set; } = string.Empty;

    [Column("PERSON_NAME")]
    [StringLength(200)]
    public string? PersonName { get; set; }

    [Column("DESIGNATION")]
    [StringLength(100)]
    public string? Designation { get; set; }

    [Column("REPORTS_TO_PF")]
    [StringLength(20)]
    public string? ReportsToPf { get; set; }

    [Column("MEMBER_ROLE")]
    [StringLength(100)]
    public string? MemberRole { get; set; }

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
