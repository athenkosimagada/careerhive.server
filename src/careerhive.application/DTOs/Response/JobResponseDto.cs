namespace careerhive.application.DTOs.Response;
public class JobResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ExternalLink { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid PostedByUserId { get; set; }
    public UserResponseDto? PostedBy { get; set; }
}
