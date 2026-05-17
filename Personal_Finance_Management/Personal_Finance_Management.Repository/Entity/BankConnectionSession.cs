using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class BankConnectionSession : BaseEntity, IAudictableEntity
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;
    public string ProviderCode { get; set; } = "casso";
    public string State { get; set; } = null!;
    public string? CodeVerifier { get; set; }
    public string? ReturnUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool AutoSync { get; set; } = true;
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
