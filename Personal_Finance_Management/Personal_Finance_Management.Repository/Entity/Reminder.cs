using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class Reminder : BaseEntity, IAudictableEntity
{
    public string Title { get; set; }
    public decimal? Amount { get; set; }
    public string Frequency { get; set; } //Ngày hay tuần hay tháng
    
    public short? DayOfMonth{ get; set; } //Này làm gì v ní
    public DateTime StartDate { get; set; }//Này làm gì v ní
    public DateTime NextDueDate { get; set; }//Này làm gì v ní 
    public string? Note{ get; set; } // Ghi chú cho người dùng sài hẻ | hay mik là người note lại cho ng dùng cái gì đó 
    public string Status{ get; set;} = "Active";  // Active | Paused | Completed | Cancelled này người dùng setup chăng
    public short NotifyDaysBefore { get; set; } = 1;
    
    //Nối với Account 
    public Guid UserId { get; set; }
    public Account User { get; set; } 
    
    //Nối với Category
    public Guid? CategoryId{ get; set; }
    public Category? Category { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}
