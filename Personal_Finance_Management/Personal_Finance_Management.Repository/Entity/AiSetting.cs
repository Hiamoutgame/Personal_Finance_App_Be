using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class AiSetting : BaseEntity<Guid>
{
    public string ModelName { get; set; } = null!;
    public string SystemPrompt { get; set; } = null!;
    public decimal Temperature { get; set; }
    public int MaxTokens { get; set; }

    public Guid? UpdatedByAdminId { get; set; }
    public Account? UpdatedByAdmin { get; set; }
    
    public string? ApiKeyEncrypted { get; set; }
    public bool IsEnabled { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}