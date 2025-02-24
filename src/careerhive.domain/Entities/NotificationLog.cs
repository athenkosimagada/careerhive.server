using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace careerhive.domain.Entities;
public class NotificationLog : BaseEntity<Guid>
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string NotificationType { get; set; } = "Email";
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
