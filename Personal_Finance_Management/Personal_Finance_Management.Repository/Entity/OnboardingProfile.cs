using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class OnboardingProfile : BaseEntity<Guid>, IAudictableEntity
{
    public decimal? MonthlyIncome { get; set; }
    public string? OccupationType { get; set; }
    public List<string> FinancialGoalTypes { get; set; }
    public string  BudgetMethodPreference { get; set; }
    public string? AgeRange { get; set; }
    public List<string> SpendingChallenges { get; set; }
    public string? RecommendedMethod { get; set; } 
    public DateTimeOffset  CompletedAt { get; set; }
    
    //Nối với Account
    public Guid UserId { get; set; }
    public Account User { get; set; } 
    
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    
}