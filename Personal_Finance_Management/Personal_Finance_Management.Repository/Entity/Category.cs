using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Category : BaseEntity<Guid>, IAudictableEntity
{
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public string? Color { get; set; }

    public bool IsDefault { get; set; }

    public Guid? OwnerUserId { get; set; }
    public Account? OwnerUser { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }


    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}