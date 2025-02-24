namespace careerhive.domain.Entities;
public class RefreshToken : BaseEntity<Guid>
{
    public string Token { get; set; } = null!;
    public DateTime ExpiryDate { get; set; }

    public string UserId { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
