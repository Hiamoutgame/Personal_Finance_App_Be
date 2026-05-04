using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class ImportTransactionDraft : BaseEntity, IAudictableEntity
{

    public int RowIndex { get; set; }

    public DateTimeOffset? TransactionDate { get; set; }
    public decimal? Amount { get; set; }

    public string? Type { get; set; }

    public string? RawDescription { get; set; }
    public string? EditedNote { get; set; }
    public bool IsValid { get; set; } = true;

    public string? ValidationError { get; set; }

    public string? NormalizedPayloadJson { get; set; }

    //Nối với importJob
    public Guid ImportJobId { get; set; }
    public ImportJob ImportJob { get; set; } = null!;

    //Nối với category
    public Guid? EditedCategoryId { get; set; }
    public Category? EditedCategory { get; set; }

    //Nối với Jar
    public Guid? EditedJarId { get; set; }
    public Jar? EditedJar { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
