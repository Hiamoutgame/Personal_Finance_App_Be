using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Jar : BaseEntity<Guid>, IAudictableEntity
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public Guid? JarSetupId { get; set; }
    public JarSetup? JarSetup { get; set; }

    public string Name { get; set; } = null!;
    public decimal? Percentage { get; set; }

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "VND";

    public string Status { get; set; } = "Active";


    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}