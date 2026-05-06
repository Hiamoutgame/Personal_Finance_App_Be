using Personal_Finance_Management.Repository.Abtraction;

namespace Personal_Finance_Management.Repository.Entity;

public class OnboardingProfile : BaseEntity, IAudictableEntity
{
    public decimal? MonthlyIncome { get; set; }
    public string? OccupationType { get; set; }
    public string? FinancialGoalTypes { get; set; }
    public string BudgetMethodPreference { get; set; } = "Undecided";
    public string? AgeRange { get; set; }
    public string? SpendingChallenges { get; set; }
    public string? RecommendedMethod { get; set; }
    public DateTimeOffset CompletedAt { get; set; }

    public Guid UserId { get; set; }
    public Account User { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
