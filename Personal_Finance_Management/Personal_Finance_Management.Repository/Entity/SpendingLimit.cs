using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class SpendingLimit : BaseEntity<Guid>, IAudictableEntity
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public Guid? JarId { get; set; }
    public Jar? Jar { get; set; }

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal LimitAmount { get; set; }

    public string Period { get; set; } = null!;

    public decimal AlertAtPercentage { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}