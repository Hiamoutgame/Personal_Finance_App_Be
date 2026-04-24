using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class AuditLog : BaseEntity<Guid>, IAudictableEntity
{
    public string ActionType { get; set; } = string.Empty; // để làm gì thé
    public string EntityType { get; set; } = string.Empty; // để làm gì thé
    public Guid? EntityId { get; set; } // Để nullable vì không phải log nào cũng gắn với 1 entity cụ thể
    public string Description { get; set; } = string.Empty;
    public string? MetadataJson { get; set; } // Lưu thông tin chi tiết dưới dạng JSON
    public string? IpAddress { get; set; }

    // Relationship: 1 AuditLog thuộc về 1 Account (Actor)
  
    public Guid ActorAccountId { get; set; } 
    public Account Account { get; set; }

 
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}