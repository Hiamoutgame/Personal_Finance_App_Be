using AuthRequest = Personal_Finance_Management.Service.Auth.Request;

namespace Personal_Finance_Management.Service.Validations;

public interface IServices
{
    Task<T> ValidateFormRequest<T>(T request);
    Task ValidateRegisterRequest(AuthRequest.RegisterRequest request);
}
