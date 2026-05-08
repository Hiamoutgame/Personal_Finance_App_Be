namespace Personal_Finance_Management.Service.limit;

public interface IService
{
    Task<Response.GetLimitsResponse> GetLimits();
    Task<Response.CreateLimitResponse> CreateLimit(Request.CreateLimitRequest request);
    Task<Response.UpdateLimitResponse> UpdateLimit(Guid id, Request.UpdateLimitRequest request);
    Task DeleteLimit(Guid id);
}