using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Jar : BaseEntity<Guid>, IAudictableEntity
{ 
    public string Name { get; set; } = null!;
    public decimal? Percentage { get; set; }

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "VND";

    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsDefault  { get; set; } = false;
    public string Status { get; set; } = "Active";
    
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; }
    
    //Nối với JarSetup
    public Guid? JarSetupId { get; set; }
    public JarSetup? JarSetup { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}