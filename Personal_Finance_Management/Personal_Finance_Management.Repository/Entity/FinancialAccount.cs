using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class FinancialAccount : BaseEntity<Guid>, IAudictableEntity
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string AccountType { get; set; } = null!;
    public string ConnectionMode { get; set; } = null!;

    public decimal CurrentBalance { get; set; }
    public decimal? AvailableBalance { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }


    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}