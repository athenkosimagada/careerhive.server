using System.ComponentModel.DataAnnotations;

namespace careerhive.application.DTOs.Request;
public class UpdateJobRequestDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "ExternalLink is required.")]
    [Url(ErrorMessage = "ExternalLink must be a valid URL.")]
    public string? ExternalLink { get; set; }
}
