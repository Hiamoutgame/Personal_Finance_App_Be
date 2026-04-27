using Personal_Finance_Management.Repository.Abtraction;
using Personal_Finance_Management.Repository.Enum;

namespace Personal_Finance_Management.Repository.Entity;

public class Role : BaseEntity, IAudictableEntity
{
    public string Code { get; set; } = null!; // để làm gì á
    public AccountRole Name { get; set; } = AccountRole.User; // mặc định là User, sau này có thể có Admin, Moderator, v.v.
    public string? Description { get; set; }

    //1 Role có nhiều Account
    public ICollection<Account> Accounts { get; set; } = new List<Account>();


    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
