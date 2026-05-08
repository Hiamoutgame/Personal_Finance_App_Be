namespace Personal_Finance_Management.Service.limit;

public interface IService
{
    public Task<Response.GetLimitsResponse> GetLimits();
    public Task<Response.CreateLimitResponse> CreateLimit(Request.CreateLimitRequest request);
    public Task<Response.UpdateLimitResponse> UpdateLimit(Guid id, Request.UpdateLimitRequest request);
    public Task DeleteLimit(Guid id);
}