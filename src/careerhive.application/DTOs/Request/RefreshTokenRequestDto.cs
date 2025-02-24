using System.ComponentModel.DataAnnotations;

namespace careerhive.application.DTOs.Request;
public class RefreshTokenRequestDto
{
    [Required]
    public string AccessToken { get; set; } = null!;

    [Required]
    public string RefreshToken { get; set; } = null!;
}
