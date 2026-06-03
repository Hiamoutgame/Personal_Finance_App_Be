namespace Personal_Finance_Management.Service.BankSync;

public interface IService
{
    Task<Response.SepayTransactionsResponse> SyncLinkedAccount(
        Guid financialAccountId,
        Request.SyncLinkedAccountRequest request);

    Task<Response.SepayTransactionsResponse> SyncLinkedAccountForUser(
        Guid financialAccountId,
        Guid userId,
        Request.SyncLinkedAccountRequest request);

    Task<Response.SepayTransactionsResponse> ProcessSepayWebhook(
        Request.SepayWebhookRequest request,
        string? authorizationHeader);
}
