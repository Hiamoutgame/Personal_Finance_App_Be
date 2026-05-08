namespace Personal_Finance_Management.Service.Jar;

public interface IService
{
    public Task<Response.GetJarsResult> GetJar();
    public Task<Response.CreateJarResponse> CreateJar(Request.CreateJarRequest request);
    public Task<Response.UpdateJarResponse> UpdateJar(Guid id, Request.UpdateJarRequest request);
    public Task<Response.DeleteJarResponse> DeleteJar(Guid id);
}