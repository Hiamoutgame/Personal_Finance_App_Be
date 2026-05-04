using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class AiSetting : BaseEntity
{
    public string ModelName { get; set; } = null!;
    public string SystemPrompt { get; set; } = null!;
    public decimal Temperature { get; set; } = 0.7m;
    public int MaxTokens { get; set; } = 1000;
    public string? ApiKeyEncrypted { get; set; }
    public bool IsEnabled { get; set; } = true;

    public Guid? UpdatedByAdminId { get; set; }
    public Account? UpdatedByAdmin { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
