using System.ComponentModel.DataAnnotations.Schema;

namespace careerhive.domain.Entities;

public class Job : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExternalLink { get; set; } = string.Empty;

    public Guid PostedByUserId { get; set; }

    [ForeignKey("PostedByUserId")]
    public User PostedBy { get; set; } = null!;
}
