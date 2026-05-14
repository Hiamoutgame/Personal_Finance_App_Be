namespace Personal_Finance_Management.Service.import
{
    public interface IServices
    {
        Task<Response.ImportImageResponse> ImportImage(Request.ImportData request);
        Task<Response.ImportJobListResponse> GetImports(Request.GetImportsRequest request);
        Task<Response.ImportJobDetailResponse> GetImport(Guid id);
        Task<Response.ImportDraftResponse> UpdateImport(Guid id, Request.UpdateImportDraftRequest request);
        Task<Response.ImportDraftResponse> UpdateImportDraft(Guid id, Guid draftId, Request.UpdateImportDraftRequest request);
        Task<Response.ConfirmImportResponse> ConfirmImport(Guid id, Request.ConfirmImportRequest request);
        Task<Response.MessageResponse> DeleteImport(Guid id);
        Task<Response.UploadedFileResponse> GetUploadedImage(string fileName);
    }
}
