using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Reminder : BaseEntity<Guid>, IAudictableEntity
{
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public string Title { get; set; } = null!;
    public decimal? Amount { get; set; }

    public string Frequency { get; set; } = null!;

    public DateTime StartDate { get; set; }
    public DateTime NextDueDate { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}