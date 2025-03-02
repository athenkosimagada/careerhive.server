namespace careerhive.domain.Entities;
public class UserSubscription : BaseEntity<Guid>
{
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
}
