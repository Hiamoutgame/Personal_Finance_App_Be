namespace Personal_Finance_Management.Service.FinancialAccount;

public interface IService
{
    public Task<List<Response.GetFinancialAccountResponse>> GetUserFinancialAccount();
    public Task<Response.CreateFinancialAccountResponse> CreateFinancialAccount(Request.CreateFinancialAccountRequest request);
    public Task<Response.UpdateFinancialAccountResponse> UpdateFinancialAccount(Request.UpdateFinancialAccountRequest request);
    public Task<Response.DeleteFinancialAccountResponse> DeleteFinancialAccount(Request.DeleteFinancialAccountRequest request);
}