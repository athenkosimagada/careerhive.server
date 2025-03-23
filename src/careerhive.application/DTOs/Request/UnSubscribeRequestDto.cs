using System.ComponentModel.DataAnnotations;

namespace careerhive.application.Request;
public class UnSubscribeRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
