using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Notification : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}