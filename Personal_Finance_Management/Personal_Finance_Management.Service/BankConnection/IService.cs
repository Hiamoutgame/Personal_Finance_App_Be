namespace Personal_Finance_Management.Service.BankConnection;

public interface IService
{
    Task<Response.StartCassoConnectionResponse> StartCassoConnection(Request.StartCassoConnectionRequest request);
    Task<Response.CassoCallbackResponse> HandleCassoCallback(string? code, string? state, string? error);
}
