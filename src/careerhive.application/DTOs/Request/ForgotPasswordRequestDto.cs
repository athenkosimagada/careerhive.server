using System.ComponentModel.DataAnnotations;

namespace careerhive.application.DTOs.Request;
public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
