namespace Personal_Finance_Management.Service.import
{
    public interface IServices
    {
        Task<Response.ImportImageResponse> ImportImage(Request.ImportData request);
    }
}
