namespace Personal_Finance_Management.Service.category
{
    public interface IService
    {
        Task<Response.GetCategoriesResponse> GetCategories();
        Task<Response.CategoryResponse> CreateCustomCategory(Request.CreateCategoryRequest request);
        Task<Response.CategoryResponse> UpdateCustomCategory(Guid id, Request.UpdateCategoryRequest request);
        Task<Response.MessageResponse> DeleteCustomCategory(Guid id);
        Task<Response.AdminCategoryResponse> CreateAdminCategory(Request.CreateAdminCategoryRequest request);
        Task<Response.AdminCategoryResponse> UpdateAdminCategory(Guid id, Request.UpdateAdminCategoryRequest request);
        Task<Response.MessageResponse> DeleteAdminCategory(Guid id);
    }
}
