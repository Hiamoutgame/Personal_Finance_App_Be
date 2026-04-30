namespace Personal_Finance_Management.Service.User;

public interface IService
{
    //Authen needed
    public Task<Response.GetUserInforResponse> GetUserInfor();
    public Task<Response.UpdateUserResponse> UpdateUserProfile(Request.UpdateUserRequest request);
    public Task<Response.ViewSetupResponse> ViewSetup();
}