using BaseResponse = Personal_Finance_Management.Service.Base.Response;

namespace Personal_Finance_Management.Service.User;

public interface IService
{
    //Authen needed
    public Task<Response.GetUserInforResponse> GetUserInfor();
    public Task<Response.UpdateUserResponse> UpdateUserProfile(Request.UpdateUserRequest request);
    public Task<Response.ViewSetupResponse> ViewSetup();
    public Task<BaseResponse.PagedResponse<Response.AdminUserResponse>> GetAdminUsers(Request.GetAdminUsersRequest request);
    public Task<Response.AdminUserResponse> GetUserInforById(Request.UserIdRequest request);
    public Task<Response.AdminUserResponse> UpdateUserStatus(Request.UserStatusRequest request);
}
