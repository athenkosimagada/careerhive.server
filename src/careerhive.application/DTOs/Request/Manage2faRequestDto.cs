using System.ComponentModel.DataAnnotations;

namespace careerhive.application.Request;
public class Manage2faRequestDto
{
    [Required]
    public bool Enable { get; set; }
}
