namespace Personal_Finance_Management.Service.Transaction;

public interface IService
{
    public Task<Response.GetTransactionsResult> GetTransactions(Request.GetTransactionsRequest request);
    public Task<Response.GetTransactionResponse> GetTransactionById(Guid id);
    public Task<Response.CreateTransactionResponse> CreateTransaction(Request.CreateTransactionRequest request);
    public Task<Response.UpdateTransactionResponse> UpdateTransaction(Guid id, Request.UpdateTransactionRequest request);
    public Task<Response.DeleteTransactionResponse> DeleteTransaction(Guid id);
}
