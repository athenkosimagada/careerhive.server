using System.ComponentModel.DataAnnotations;

namespace careerhive.application.DTOs.Request;
public class Manage2faRequestDto
{
    [Required]
    public bool Enable { get; set; }
}
