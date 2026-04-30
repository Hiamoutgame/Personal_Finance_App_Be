using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class FinancialAccount : BaseEntity, IAudictableEntity
{
    public string Name { get; set; } = null!;
    public string AccountType { get; set; } = null!;
    public required string ConnectionMode { get; set; }  // Manual | LinkedApi // 36000
    public string? ProviderCode { get; set; }
    public string? ProviderName { get; set; }
    public string? ExternalAccountId { get; set; }
    public string? ExternalAccountRef { get; set; }
    public string? MaskedAccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string Currency { get; set; } = "VND";
    public decimal CurrentBalance { get; set; } = 0;
    public decimal? AvailableBalance { get; set; }
    public DateTimeOffset? BalanceAsOf { get; set; }
    public string SyncStatus { get; set; } = "NeverSynced";
    public DateTimeOffset? LastSyncedAt { get; set; }
    public string? LastSyncError { get; set; }
    public string? AccessTokenRef { get; set; }
    public DateTimeOffset? TokenExpiresAt { get; set; }
    public DateTimeOffset? ConsentExpiresAt { get; set; }
    public string? LastSyncCursor { get; set; }
    public string? WebhookSubscriptionId { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
