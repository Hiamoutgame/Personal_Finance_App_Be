using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Reminder : BaseEntity, IAudictableEntity
{
    public string Title { get; set; } = null!;
    public decimal? Amount { get; set; }
    public string? Frequency { get; set; }

    public short? DayOfMonth { get; set; }
    public DateTime StartDate { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "Active";
    public short? NotifyDaysBefore { get; set; } = 1;

    //Nối với Account 
    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    //Nối với Category
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
