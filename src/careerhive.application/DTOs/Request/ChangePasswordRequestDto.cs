using System.ComponentModel.DataAnnotations;

namespace careerhive.application.DTOs.Request;
public class ChangePasswordRequestDto
{
    [Required]
    public string OldPassword { get; set; } = null!;

    [Required]
    public string NewPassword { get; set; } = null!;

    [Required]
    [Compare("NewPassword", ErrorMessage = "The new password and confirm new password do not match.")]
    public string ConfirmNewPassword { get; set; } = null!;
}
