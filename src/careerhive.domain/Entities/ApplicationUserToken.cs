namespace careerhive.domain.Entities;
public class ApplicationUserToken : BaseEntity<Guid>
{
    public string TokenType { get; set; } = string.Empty;
    public string TokenValue { get; set; } = string.Empty;  
    public DateTime? ExpiryTime { get; set; }
    public DateTime? UsedTime { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
