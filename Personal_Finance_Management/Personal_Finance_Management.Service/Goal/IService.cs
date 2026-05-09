

namespace Personal_Finance_Management.Service.goal;

public interface IService
{
    public Task<Response.GetGoalsResponse> GetGoals();
    public Task<Response.GetGoalByIdResponse> GetGoalById(Guid id);
    public Task<Response.CreateGoalResponse> CreateGoal(Request.CreateGoalRequest request);
    public Task<Response.UpdateGoalResponse> UpdateGoal(Guid id, Request.UpdateGoalRequest request);
    public Task DeleteGoal(Guid id);
}