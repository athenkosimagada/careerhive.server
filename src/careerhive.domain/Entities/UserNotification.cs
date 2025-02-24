namespace careerhive.domain.Entities;
public class UserNotification
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string NotificationType { get; set; } = "Email";
    public bool IsEnabled { get; set; }
}
