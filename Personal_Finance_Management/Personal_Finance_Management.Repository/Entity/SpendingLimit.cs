using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class SpendingLimit : BaseEntity, IAudictableEntity
{
    public decimal LimitAmount { get; set; }
    public string Period { get; set; } = null!;
    public decimal AlertAtPercentage { get; set; }
    public bool IsActive { get; set; } = true;

    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    //Nối với Jar
    public Guid? JarId { get; set; }
    public Jar? Jar { get; set; }

    //Nối với Category
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
