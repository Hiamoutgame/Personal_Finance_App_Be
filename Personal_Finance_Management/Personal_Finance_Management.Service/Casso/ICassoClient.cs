namespace Personal_Finance_Management.Service.Casso;

public interface ICassoClient
{
    string BuildAuthorizationUrl(string state, string redirectUri);
    Task<CassoTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CassoAccount>> GetAccountsAsync(
        string? accessToken,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CassoTransactionRecord>> GetAccountTransactionsAsync(
        string? accessToken,
        string accountId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CassoTransactionRecord>> GetTransactionsAsync(
        string? accessToken,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken = default);
    Task TriggerSyncAsync(
        string? accessToken,
        string accountNumber,
        CancellationToken cancellationToken = default);
}
