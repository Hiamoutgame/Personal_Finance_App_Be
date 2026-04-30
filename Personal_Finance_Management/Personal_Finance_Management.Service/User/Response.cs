using Personal_Finance_Management.Repository.Entity;

namespace Personal_Finance_Management.Service.User;

public class Response
{
    public class GetUserInforResponse
    {
        public Guid Id { get; set; }
        public string userName { get; set; }
        public string fullName  { get; set; }
        public string email { get; set; }
        public string phone  { get; set; }
        public string avatarUrl { get; set; }
        public string preferredCurrency { get; set; }
        public bool isOnboardingCompleted { get; set; }
    }

    public class UpdateUserResponse
    {
        public Guid Id { get; set; }
        public string fullName { get; set; }
        public string phone { get; set; }
        public string avatarUrl { get; set; }
    }

    public class ViewSetupResponse
    {
        public bool isOnboardingCompleted { get; set; }
        public decimal? monthlyIncome  { get; set; }
        public string budgetMethod { get; set; }
        public Guid defaultFinancialAccountId { get; set; }
        public int jarCount { get; set; }
        public int financialAccountCount  { get; set; }
        public int limitCount { get; set; }
        public int activeGoalCount { get; set; }
    }
}