using System.ComponentModel.DataAnnotations;

namespace careerhive.application.Request;
public class SubscribeRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
