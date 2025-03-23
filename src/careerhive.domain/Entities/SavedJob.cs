using System.ComponentModel.DataAnnotations.Schema;

namespace careerhive.domain.Entities;
public class SavedJob
{
    public Guid Id { get; set; }

    public Guid SavedByUserId { get; set; }
    [ForeignKey(nameof(SavedByUserId))]
    public User SavedBy { get; set; } = null!;

    public Guid JobId { get; set; }
    [ForeignKey(nameof(JobId))]
    public Job SavedJobDetails { get; set; } = null!;
}
