using System.ComponentModel.DataAnnotations;

namespace careerhive.application.DTOs.Request;
public class UnSubscribeRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
