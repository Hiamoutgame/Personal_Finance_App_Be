using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarSetup : BaseEntity, IAudictableEntity
{
    public string MethodType { get; set; } = null!;

    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;
    public ICollection<Jar> Jars { get; set; } = new List<Jar>();

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
