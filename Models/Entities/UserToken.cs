using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("USER_TOKEN")]
public class UserToken
{
    [Key]
    [Column("REF_NO")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RefNo { get; set; }

    [Column("USERID")]
    [Required, StringLength(60)]
    public string UserId { get; set; } = string.Empty;

    [Column("USERNAME")]
    [StringLength(150)]
    public string? UserName { get; set; }

    [Column("LASTTOKEN")]
    [StringLength(2000)]
    public string? LastToken { get; set; }

    [Column("HASH_TOKEN")]
    [StringLength(200)]
    public string? HashToken { get; set; }

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; }
}
