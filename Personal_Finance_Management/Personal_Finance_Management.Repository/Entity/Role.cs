using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Role: BaseEntity<short>, IAudictableEntity
{
    public string Code { get; set; } = null!; // để làm gì á
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    //1 Role có nhiều Account
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}