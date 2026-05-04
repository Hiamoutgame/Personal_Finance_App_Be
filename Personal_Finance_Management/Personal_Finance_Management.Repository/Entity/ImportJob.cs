using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class ImportJob : BaseEntity
{
    public string FileName { get; set; } = null!;
    public string? OriginalContentType { get; set; }
    public string StoredFilePath { get; set; } = null!;
    public string? BankCode { get; set; }
    public string Status { get; set; } = "Pending";
    public int Progress { get; set; } = 0;
    public int? EstimatedRows { get; set; }
    public int ParsedCount { get; set; } = 0;
    public int FailedCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }

    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public Guid FinancialAccountId { get; set; }
    public FinancialAccount FinancialAccount { get; set; } = null!;

    public ICollection<ImportTransactionDraft> Drafts { get; set; } = new List<ImportTransactionDraft>();

    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
