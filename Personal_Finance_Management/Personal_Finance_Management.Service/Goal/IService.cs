using Personal_Finance_Management.Service.Goal;

namespace Personal_Finance_Management.Service.goal;

public interface IService
{
    public Task<Response.GetGoalsResponse> GetGoals();
    
    public Task<Response.GetGoalsResponse> GetGoalById(Guid id);
}