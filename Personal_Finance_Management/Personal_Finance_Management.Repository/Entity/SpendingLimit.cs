using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class SpendingLimit : BaseEntity<Guid>, IAudictableEntity
{
    public decimal LimitAmount { get; set; }

    public string Period { get; set; } // này là gì nưa dị

    public decimal AlertAtPercentage { get; set; }

    public bool IsActive { get; set; } = true; // phải còn hoạt động mới đc

    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; }
    
    //Nối với Jar
    public Guid? JarId { get; set; }
    public Jar? Jar { get; set; }
    
    //Nối với Category
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
}