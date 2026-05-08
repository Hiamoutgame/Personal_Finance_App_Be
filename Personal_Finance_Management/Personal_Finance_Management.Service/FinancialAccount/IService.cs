namespace Personal_Finance_Management.Service.FinancialAccount;

public interface IService
{
    public Task<Response.GetFinancialAccountResult> GetUserFinancialAccount();
    public Task<Response.CreateManualFinancialAccountResponse> CreateManualFinancialAccount(Request.CreateManualFinancialAccountRequest request);
    public Task<Response.CreateLinkApiFinancialAccountResponse> CreateLinkApiFinancialAccount(Request.CreateLinkApiFinancialAccountRequest request);
    public Task<Response.UpdateFinancialAccountResponse> UpdateFinancialAccount(Guid id, Request.UpdateFinancialAccountRequest request);
    public Task<Response.DeleteFinancialAccountResponse> DeleteFinancialAccount(Guid id);
}
