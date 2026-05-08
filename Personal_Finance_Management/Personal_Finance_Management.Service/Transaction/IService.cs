namespace Personal_Finance_Management.Service.Transaction;

public interface IService
{
    public Task<Response.GetTransactionsResult> GetTransactions(Request.GetTransactionsRequest request);
    public Task<Response.CreateTransactionResponse> CreateTransaction(Request.CreateTransactionRequest request);
    public Task<Response.UpdateTransactionResponse> UpdateTransaction(Guid id, Request.UpdateTransactionRequest request);
    public Task<Response.DeleteTransactionResponse> DeleteTransaction(Guid id);
    public Task<Response.CassoTransactionsResponse> ProcessCassoWebhook(
        Request.CassoWebhookRequest request,
        string? secureToken,
        string? cassoSignature);
    public Task<Response.CassoTransactionsResponse> SyncCassoTransactions(Request.CassoSyncTransactionsRequest request);
}
