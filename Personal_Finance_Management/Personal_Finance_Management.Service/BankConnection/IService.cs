namespace Personal_Finance_Management.Service.BankConnection;

public interface IService
{
    Task<Response.StartSepayConnectionResponse> StartSepayConnection(Request.StartSepayConnectionRequest request);
    Task<Response.SepayCallbackResponse> HandleSepayCallback(string? code, string? state, string? error);
}
