namespace Personal_Finance_Management.Service.Auth;

public interface IService
{
    Task<Response.RegisterResponse> Register(Request.RegisterRequest request);
}
