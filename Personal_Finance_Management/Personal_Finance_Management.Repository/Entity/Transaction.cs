using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Transaction : BaseEntity<Guid>, IAudictableEntity
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;

    public Guid? JarId { get; set; }
    public Guid? CategoryId { get; set; }

    public string Type { get; set; } = null!;
    public decimal Amount { get; set; }

    public DateTimeOffset TransactionDate { get; set; }

    public string SourceType { get; set; } = "Manual";


    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}