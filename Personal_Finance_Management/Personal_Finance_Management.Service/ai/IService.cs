namespace Personal_Finance_Management.Service.AI;

public interface IService
{
    public Task<Response.AnswerResponse> GetAiSettings(Request.ChatBoxRequest request);
    public Task<Response.AdminAiSettingsResponse> GetAdminAiSettings();
    public Task<Response.UpdateAiSettingsResponse> UpdateAdminAiSettings(Request.UpdateAiSettingsRequest request);
}
