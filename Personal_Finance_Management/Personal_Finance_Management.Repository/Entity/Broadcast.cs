using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Broadcast : BaseEntity, IAudictableEntity
{
    // thông báo gửi đến toàn user hệ thống
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string TargetAudience { get; set; } = "All";
    public string Status { get; set; } = "Queued";

    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }

    public int TargetCount { get; set; } = 0;
    public int DeliveredCount { get; set; } = 0;

    // Nối với Account
    public Guid CreatedByAdminId { get; set; }
    public Account CreatedByAdmin { get; set; } = null!;

    //Nối với Notification
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
