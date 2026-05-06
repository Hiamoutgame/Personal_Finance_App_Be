namespace Personal_Finance_Management.Service.Onboarding;

public class Request
{
    public class FillOnboardingRequest
    {
        public int monthlyIncome { get; set; }
        public string occupationType  { get; set; }
        public List<string> financialGoalTypes { get; set; }
        public string budgetMethodPreference { get; set; }
        public string ageRange  { get; set; }
        public List<string> spendingChallenges  { get; set; }
    }
}