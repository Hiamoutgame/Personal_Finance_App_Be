using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class JarTransfer : BaseEntity, IAudictableEntity
{
    // FK → accounts
    public Guid FromJarId { get; set; }   // FK → jars
    public Guid ToJarId { get; set; }   // FK → jars
    public decimal Amount { get; set; }   // > 0
    public string? Note { get; set; }


    //Nối với Account
    public Guid UserId { get; set; }
        public Account User { get; set; } = null!;

    //Nối tới hủ Jar cả 2 luôn
    public Jar FromJar { get; set; } = null!;
    public Jar ToJar { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
