namespace Personal_Finance_Management.Service.Onboarding;

public interface IService
{
    public Task<Response.OnboardingResponse> CreateOnboarding (Request.FillOnboardingRequest request);
}