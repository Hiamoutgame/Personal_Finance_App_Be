namespace Personal_Finance_Management.Service.FinancialAccount;

public interface IService
{
    public Task<Response.GetFinancialAccountResult> GetUserFinancialAccount();
    public Task<Response.CreateFinancialAccountResponse> CreateFinancialAccount(Request.CreateFinancialAccountRequest request);
    public Task<Response.UpdateFinancialAccountResponse> UpdateFinancialAccount(Guid id, Request.UpdateFinancialAccountRequest request);
    public Task<Response.DeleteFinancialAccountResponse> DeleteFinancialAccount(Guid id);
}