namespace Personal_Finance_Management.Service.BankSync;

public interface IService
{
    Task<Response.CassoTransactionsResponse> SyncLinkedAccount(
        Guid financialAccountId,
        Request.SyncLinkedAccountRequest request);

    Task<Response.CassoTransactionsResponse> SyncLinkedAccountForUser(
        Guid financialAccountId,
        Guid userId,
        Request.SyncLinkedAccountRequest request);

    Task<Response.CassoTransactionsResponse> ProcessCassoWebhook(
        Request.CassoWebhookRequest request,
        string? secureToken,
        string? cassoSignature);
}
