namespace careerhive.domain.Entities;
public class InvalidToken
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryTime { get; set; }
}
