namespace Personal_Finance_Management.Service.Onboarding;

public class Response
{
    public class OnboardingResponse
    {
        public string recommendedMethod { get; set; }
        public List<Category> recommendedCategories { get; set; }
        public List<Jar> recommendedJars { get; set; }
        public defaultFAccount defaultFinancialAccount  { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public string icon { get; set; }
    }

    public class Jar
    {
        public string name { get; set; }
        public int percentage { get; set; }
    }
    
    public class defaultFAccount
    {
        public string name { get; set; }
        public string accountType { get; set; }
    }
}