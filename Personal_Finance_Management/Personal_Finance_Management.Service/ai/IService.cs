namespace Personal_Finance_Management.Service.AI;

public interface IService
{
    public Task<Response.AnswerResponse> ChatBot(Request.ChatBoxRequest request);
    public Task<Response.AdminAiSettingsResponse> GetAdminAiSettings();
    public Task<Response.UpdateAiSettingsResponse> UpdateAdminAiSettings(Request.UpdateAiSettingsRequest request);
}
