using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("APP_USER")]
public class AppUser
{
    [Key]
    [Column("USER_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long UserId { get; set; }

    [Column("PF_NO")]
    [Required, StringLength(20)]
    public string PfNo { get; set; } = string.Empty;

    [Column("USER_NAME")]
    [StringLength(200)]
    public string? UserName { get; set; }

    [Column("DESIGNATION")]
    [StringLength(100)]
    public string? Designation { get; set; }

    [Column("DEPT_ID")]
    public long? DeptId { get; set; }

    [Column("ROLE_CODE")]
    [Required, StringLength(20)]
    public string RoleCode { get; set; } = string.Empty;

    [Column("AD_LOGIN_ID")]
    [StringLength(200)]
    public string? AdLoginId { get; set; }

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
}
