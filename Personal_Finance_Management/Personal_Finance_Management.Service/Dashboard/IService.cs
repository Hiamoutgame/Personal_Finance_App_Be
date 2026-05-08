namespace Personal_Finance_Management.Service.Dashboard;

public interface IService
{
    public Task<Response.GetDashboardResult> GetDashboard();
}