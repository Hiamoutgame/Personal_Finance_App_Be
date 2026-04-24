using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarTransfer : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public Guid FromJarId { get; set; }
    public Jar FromJar { get; set; } = null!;

    public Guid ToJarId { get; set; }
    public Jar ToJar { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}