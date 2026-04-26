using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Broadcast : BaseEntity<Guid>, IAudictableEntity
{
    //Ko biết chức năng của bảng này
    public string Title { get; set; }
    public string Body { get; set; }
    public string TargetAudience { get; set; }
    public string Status { get; set; } // có những trạng thái gì có thể xảy ra
    
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    
    public int TargetCount { get; set; }
    public int DeliveredCount { get; set; }

    // Nối với Account
    public Guid CreatedByAdminId { get; set; }
    public Account CreatByAdmin { get; set; } = null!;
    
    //Nối với Notification
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}