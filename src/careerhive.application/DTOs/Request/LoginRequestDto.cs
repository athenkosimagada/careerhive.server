using System.ComponentModel.DataAnnotations;

namespace careerhive.application.DTOs.Request;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}
