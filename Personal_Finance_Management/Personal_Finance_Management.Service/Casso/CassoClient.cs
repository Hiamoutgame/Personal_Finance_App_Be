using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Casso;

public class CassoClient : ICassoClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public CassoClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public string BuildAuthorizationUrl(string state, string redirectUri)
    {
        var clientId = ResolveRequired("Casso:ClientId", "Casso OAuth client id is not configured.");
        var authorizationUrl = Resolve("Casso:AuthorizationUrl")
                               ?? "https://oauth.casso.vn/auth/authorize";
        var scope = Resolve("Casso:Scope") ?? "webhook transaction";

        var queryParams = new Dictionary<string, string?>
        {
            ["client_id"] = clientId,
            ["scope"] = scope,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["state"] = state
        };

        return authorizationUrl
               + (authorizationUrl.Contains('?') ? "&" : "?")
               + string.Join("&", queryParams
                   .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                   .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
    }

    public async Task<CassoTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var clientId = ResolveRequired("Casso:ClientId", "Casso OAuth client id is not configured.");
        var clientSecret = ResolveRequired("Casso:ClientSecret", "Casso OAuth client secret is not configured.");
        var tokenUrl = Resolve("Casso:TokenUrl") ?? "https://oauth.casso.vn/auth/token";

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
        var basicToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicToken);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["code"] = code
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw AppValidationException.BadRequest("Casso token exchange failed.", "code", "CASSO_TOKEN_EXCHANGE_FAILED");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;
        var accessToken = ReadString(root, "access_token") ?? ReadString(root, "accessToken");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw AppValidationException.BadRequest("Casso token response is invalid.", "access_token", "CASSO_TOKEN_INVALID");
        }

        var expiresIn = ReadInt(root, "expires_in") ?? ReadInt(root, "expiresIn");
        return new CassoTokenResponse
        {
            accessToken = accessToken,
            refreshToken = ReadString(root, "refresh_token") ?? ReadString(root, "refreshToken"),
            tokenType = ReadString(root, "token_type") ?? ReadString(root, "tokenType") ?? "bearer",
            expiresIn = expiresIn,
            expiresAt = expiresIn.HasValue ? DateTimeOffset.UtcNow.AddSeconds(expiresIn.Value) : null
        };
    }

    public async Task<IReadOnlyList<CassoAccount>> GetAccountsAsync(
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUrl("accounts"));
        ApplyAuthorization(request, accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw AppValidationException.BadRequest("Casso accounts request failed.", "casso", "CASSO_ACCOUNTS_FAILED");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        EnsureCassoSuccess(json.RootElement);

        var accounts = new List<CassoAccount>();
        var data = json.RootElement.GetProperty("data");
        if (data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                var parsed = ParseAccount(item);
                if (parsed != null)
                {
                    accounts.Add(parsed);
                }
            }
        }
        else if (data.ValueKind == JsonValueKind.Object)
        {
            var parsed = ParseAccount(data);
            if (parsed != null)
            {
                accounts.Add(parsed);
            }
        }

        return accounts;
    }

    public async Task<IReadOnlyList<CassoTransactionRecord>> GetAccountTransactionsAsync(
        string? accessToken,
        string accountId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken = default)
    {
        var path = $"accounts/{Uri.EscapeDataString(accountId)}/transactions";
        return await GetTransactionsFromUrl(
            accessToken,
            BuildApiUrl(path, BuildTransactionQuery(fromDate, toDate, page, pageSize, sort)),
            cancellationToken);
    }

    public async Task<IReadOnlyList<CassoTransactionRecord>> GetTransactionsAsync(
        string? accessToken,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken = default)
    {
        return await GetTransactionsFromUrl(
            accessToken,
            BuildApiUrl("transactions", BuildTransactionQuery(fromDate, toDate, page, pageSize, sort)),
            cancellationToken);
    }

    public async Task TriggerSyncAsync(
        string? accessToken,
        string accountNumber,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildApiUrl("sync"));
        ApplyAuthorization(request, accessToken);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { bank_acc_id = accountNumber }),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw AppValidationException.BadRequest("Casso provider sync failed.", "casso", "CASSO_PROVIDER_SYNC_FAILED");
        }
    }

    private async Task<IReadOnlyList<CassoTransactionRecord>> GetTransactionsFromUrl(
        string? accessToken,
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        ApplyAuthorization(request, accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw AppValidationException.BadRequest("Casso transactions request failed.", "casso", "CASSO_TRANSACTIONS_FAILED");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        EnsureCassoSuccess(json.RootElement);

        var records = new List<CassoTransactionRecord>();
        if (!json.RootElement.TryGetProperty("data", out var data))
        {
            return records;
        }

        if (data.ValueKind == JsonValueKind.Object
            && data.TryGetProperty("records", out var recordsElement)
            && recordsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in recordsElement.EnumerateArray())
            {
                records.Add(new CassoTransactionRecord { payload = item.Clone() });
            }
        }
        else if (data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                records.Add(new CassoTransactionRecord { payload = item.Clone() });
            }
        }

        return records;
    }

    private CassoAccount? ParseAccount(JsonElement item)
    {
        var externalId = ReadFlexibleString(item, "id")
                         ?? ReadFlexibleString(item, "bankAccId")
                         ?? ReadFlexibleString(item, "bank_acc_id");
        if (string.IsNullOrWhiteSpace(externalId))
        {
            return null;
        }

        var accountNumber = ReadString(item, "accountNumber")
                            ?? ReadString(item, "bankSubAccId")
                            ?? ReadString(item, "bank_sub_acc_id")
                            ?? ReadString(item, "subAccId");
        var holderName = ReadString(item, "accountName")
                         ?? ReadString(item, "bankAccountName")
                         ?? ReadString(item, "accountHolderName");
        var bankName = ReadString(item, "bankName");
        var bankCode = ReadString(item, "bankCode");
        if (item.TryGetProperty("bank", out var bank) && bank.ValueKind == JsonValueKind.Object)
        {
            bankName ??= ReadString(bank, "name") ?? ReadString(bank, "shortName") ?? ReadString(bank, "codeName");
            bankCode ??= ReadFlexibleString(bank, "bin") ?? ReadString(bank, "code") ?? ReadString(bank, "codeName");
        }

        var name = holderName
                   ?? bankName
                   ?? (string.IsNullOrWhiteSpace(accountNumber) ? "Casso bank account" : $"Casso {ServiceTextHelper.MaskTrailing(accountNumber)}");

        return new CassoAccount
        {
            externalId = externalId,
            accountNumber = accountNumber,
            name = name,
            bankName = bankName,
            bankCode = bankCode,
            accountHolderName = holderName,
            balance = ReadDecimal(item, "balance")
                      ?? ReadDecimal(item, "currentBalance")
                      ?? ReadDecimal(item, "availableBalance"),
            rawJson = item.GetRawText()
        };
    }

    private void ApplyAuthorization(HttpRequestMessage request, string? accessToken)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            return;
        }

        var apiKey = Resolve("Casso:ApiKey") ?? Resolve("CasooOptions:ApiKey");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw AppValidationException.BadRequest("Casso credential is not configured.", "Casso:ApiKey", "CASSO_CONFIG_MISSING");
        }

        var value = apiKey.Trim();
        if (value.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(value);
            return;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Apikey", value);
    }

    private string BuildApiUrl(string path, IEnumerable<string>? queryParams = null)
    {
        var baseUrl = (Resolve("Casso:BaseUrl") ?? "https://oauth.casso.vn/v2").TrimEnd('/');
        var url = $"{baseUrl}/{path.TrimStart('/')}";
        var query = queryParams?.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        return query is { Count: > 0 } ? $"{url}?{string.Join("&", query)}" : url;
    }

    private static IEnumerable<string> BuildTransactionQuery(
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort)
    {
        yield return $"page={page}";
        yield return $"pageSize={pageSize}";
        yield return $"sort={Uri.EscapeDataString(string.IsNullOrWhiteSpace(sort) ? "ASC" : sort.Trim().ToUpperInvariant())}";
        if (fromDate.HasValue)
        {
            yield return $"fromDate={fromDate.Value:yyyy-MM-dd}";
        }
        if (toDate.HasValue)
        {
            yield return $"toDate={toDate.Value:yyyy-MM-dd}";
        }
    }

    private static void EnsureCassoSuccess(JsonElement root)
    {
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Number && error.GetInt32() != 0)
        {
            throw AppValidationException.BadRequest("Casso response is invalid.", "casso", "CASSO_RESPONSE_INVALID");
        }

        if (!root.TryGetProperty("data", out _))
        {
            throw AppValidationException.BadRequest("Casso response is missing data.", "casso", "CASSO_RESPONSE_INVALID");
        }
    }

    private string ResolveRequired(string key, string message)
    {
        var value = Resolve(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw AppValidationException.BadRequest(message, key, "CASSO_CONFIG_MISSING");
        }

        return value;
    }

    private string? Resolve(string key)
    {
        var value = _configuration[key];
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return key.StartsWith("Casso:", StringComparison.OrdinalIgnoreCase)
            ? _configuration["CasooOptions:" + key["Casso:".Length..]]
            : null;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? ReadFlexibleString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static int? ReadInt(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetInt32()
            : null;
    }

    private static decimal? ReadDecimal(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDecimal()
            : null;
    }
}
