namespace Personal_Finance_Management.Service.Sepay;

public interface ISepayClient
{
    string BuildAuthorizationUrl(string state, string redirectUri);

    Task<SepayTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default);

    Task<SepayTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SepayAccount>> GetAccountsAsync(
        string? accessToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SepayTransactionRecord>> GetAccountTransactionsAsync(
        string? accessToken,
        string accountId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SepayTransactionRecord>> GetTransactionsAsync(
        string? accessToken,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken = default);
}
