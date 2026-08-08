using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT_BUDGET_MONITORING_PORTAL.Models.Entities;

[Table("LOGIN_CAPTCHA_QUESTION")]
public class LoginCaptchaQuestion
{
    [Key]
    [Column("Q_ID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long QId { get; set; }

    [Column("QUESTION_TEXT")]
    [Required, StringLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    [Column("ANSWER_TEXT")]
    [Required, StringLength(200)]
    public string AnswerText { get; set; } = string.Empty;

    [Column("IS_ACTIVE")]
    [StringLength(1)]
    public string IsActive { get; set; } = "Y";
}
