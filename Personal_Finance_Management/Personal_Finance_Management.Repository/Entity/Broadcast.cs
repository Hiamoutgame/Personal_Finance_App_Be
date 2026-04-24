using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Broadcast : BaseEntity<Guid>, IAudictableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    
    public DateTimeOffset? ScheduledAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    
    // Các trường số lượng (NN - Not Null)
    public int TargetCount { get; set; }
    public int DeliveredCount { get; set; }

    // Relationship: Được tạo bởi 1 Admin (Account)
    public Guid CreatedByAdminId { get; set; }
    public virtual Account Admin { get; set; } = null!;

    // Thực thi từ IAuditableEntity
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}