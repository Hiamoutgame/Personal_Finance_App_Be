namespace Personal_Finance_Management.Repository.Entity;

public class Role
{
    public short Id { get; set; }
    public string Code { get; set; } = null!; // để làm gì á
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    //1 Role có nhiều Account
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}