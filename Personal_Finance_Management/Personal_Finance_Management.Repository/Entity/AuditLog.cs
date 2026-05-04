using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class AuditLog : BaseEntity
{
    public string ActionType { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid? EntityId { get; set; }
    public string Description { get; set; } = null!;
    public string? MetadataJson { get; set; }
    public string? IpAddress { get; set; }

    public Guid ActorAccountId { get; set; }
    public Account Account { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}
