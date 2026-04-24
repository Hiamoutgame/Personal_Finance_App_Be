using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class ImportTransactionDraft : BaseEntity<Guid>, IAudictableEntity
{
    public Guid ImportJobId { get; set; }
    public ImportJob ImportJob { get; set; } = null!;

    public int RowIndex { get; set; }

    public DateTimeOffset? TransactionDate { get; set; }
    public decimal? Amount { get; set; }

    public string? Type { get; set; }

    public string? RawDescription { get; set; }

    public string? SuggestedNote { get; set; }

    public Guid? SuggestedCategoryId { get; set; }
    public Category? SuggestedCategory { get; set; }

    public Guid? SuggestedJarId { get; set; }
    public Jar? SuggestedJar { get; set; }

    public bool IsValid { get; set; }

    public string? ValidationError { get; set; }

    public string? NormalizedPayloadJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}