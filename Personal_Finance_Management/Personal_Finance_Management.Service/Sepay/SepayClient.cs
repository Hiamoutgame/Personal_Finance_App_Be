using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Service.Base;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Sepay;

public class SepayClient : ISepayClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SepayClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public string BuildAuthorizationUrl(string state, string redirectUri)
    {
        var clientId = ResolveRequired(ConfigKeys.Sepay.ClientId, "SePay OAuth client id is not configured.");
        var authorizationUrl = _configuration[ConfigKeys.Sepay.AuthorizationUrl]
                               ?? "https://my.sepay.vn/oauth/authorize";
        var scope = _configuration[ConfigKeys.Sepay.Scope] ?? "bank-account:read transaction:read";

        var queryParams = new Dictionary<string, string?>
        {
            ["response_type"] = "code",
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["scope"] = scope,
            ["state"] = state
        };

        return authorizationUrl
               + (authorizationUrl.Contains('?') ? "&" : "?")
               + string.Join("&", queryParams
                   .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                   .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));
    }

    public async Task<SepayTokenResponse> ExchangeCodeForTokenAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var clientId = ResolveRequired(ConfigKeys.Sepay.ClientId, "SePay OAuth client id is not configured.");
        var clientSecret = ResolveRequired(ConfigKeys.Sepay.ClientSecret, "SePay OAuth client secret is not configured.");
        var tokenUrl = _configuration[ConfigKeys.Sepay.TokenUrl] ?? "https://my.sepay.vn/oauth/token";

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };

        return await PostTokenForm(tokenUrl, form, ErrorMessages.SepayTokenExchangeFailed, ErrorCodes.SepayTokenExchangeFailed, "code", cancellationToken);
    }

    public async Task<SepayTokenResponse> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var clientId = ResolveRequired(ConfigKeys.Sepay.ClientId, "SePay OAuth client id is not configured.");
        var clientSecret = ResolveRequired(ConfigKeys.Sepay.ClientSecret, "SePay OAuth client secret is not configured.");
        var tokenUrl = _configuration[ConfigKeys.Sepay.TokenUrl] ?? "https://my.sepay.vn/oauth/token";

        var form = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };

        return await PostTokenForm(tokenUrl, form, ErrorMessages.SepayTokenRefreshFailed, ErrorCodes.SepayTokenRefreshFailed, "refresh_token", cancellationToken);
    }

    public async Task<IReadOnlyList<SepayAccount>> GetAccountsAsync(
        string? accessToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildApiUrl("bank-account"));
        ApplyAuthorization(request, accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayAccountsFailed, "sepay", ErrorCodes.SepayAccountsFailed);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;

        var accounts = new List<SepayAccount>();
        var data = ResolveDataElement(root);
        if (data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                var parsed = ParseAccount(item);
                if (parsed != null) accounts.Add(parsed);
            }
        }
        else if (data.ValueKind == JsonValueKind.Object)
        {
            var parsed = ParseAccount(data);
            if (parsed != null) accounts.Add(parsed);
        }

        return accounts;
    }

    public async Task<IReadOnlyList<SepayTransactionRecord>> GetAccountTransactionsAsync(
        string? accessToken,
        string accountId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken = default)
    {
        var queryParams = BuildTransactionQuery(fromDate, toDate, page, pageSize, sort).ToList();
        queryParams.Add($"account_id={Uri.EscapeDataString(accountId)}");
        return await GetTransactionsFromUrl(
            accessToken,
            BuildApiUrl("transaction", queryParams),
            cancellationToken);
    }

    public async Task<IReadOnlyList<SepayTransactionRecord>> GetTransactionsAsync(
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
            BuildApiUrl("transaction", BuildTransactionQuery(fromDate, toDate, page, pageSize, sort)),
            cancellationToken);
    }

    private async Task<SepayTokenResponse> PostTokenForm(
        string tokenUrl,
        Dictionary<string, string> form,
        string errorMessage,
        string errorCode,
        string errorField,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(form)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw AppValidationException.BadRequest(errorMessage, errorField, errorCode);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;
        var accessToken = ReadString(root, "access_token") ?? ReadString(root, "accessToken");
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayTokenResponseInvalid, "access_token", ErrorCodes.SepayTokenInvalid);
        }

        var expiresIn = ReadInt(root, "expires_in") ?? ReadInt(root, "expiresIn");
        return new SepayTokenResponse
        {
            accessToken = accessToken,
            refreshToken = ReadString(root, "refresh_token") ?? ReadString(root, "refreshToken"),
            tokenType = ReadString(root, "token_type") ?? ReadString(root, "tokenType") ?? "Bearer",
            expiresIn = expiresIn,
            expiresAt = expiresIn.HasValue ? DateTimeOffset.UtcNow.AddSeconds(expiresIn.Value) : null
        };
    }

    private async Task<IReadOnlyList<SepayTransactionRecord>> GetTransactionsFromUrl(
        string? accessToken,
        string requestUri,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        ApplyAuthorization(request, accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw AppValidationException.BadRequest(ErrorMessages.SepayTransactionsFailed, "sepay", ErrorCodes.SepayTokenInvalid);
            }
            throw AppValidationException.BadRequest(ErrorMessages.SepayTransactionsFailed, "sepay", ErrorCodes.SepayTransactionsFailed);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;

        var records = new List<SepayTransactionRecord>();
        var data = ResolveDataElement(root);

        if (data.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in data.EnumerateArray())
            {
                records.Add(new SepayTransactionRecord { payload = item.Clone() });
            }
        }
        else if (data.ValueKind == JsonValueKind.Object
            && data.TryGetProperty("records", out var recordsElement)
            && recordsElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in recordsElement.EnumerateArray())
            {
                records.Add(new SepayTransactionRecord { payload = item.Clone() });
            }
        }

        return records;
    }

    private static JsonElement ResolveDataElement(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array) return root;
        if (root.TryGetProperty("data", out var data)) return data;
        if (root.TryGetProperty("items", out var items)) return items;
        return root;
    }

    private SepayAccount? ParseAccount(JsonElement item)
    {
        var externalId = ReadFlexibleString(item, "xid")
                         ?? ReadFlexibleString(item, "id")
                         ?? ReadFlexibleString(item, "bankAccId");
        if (string.IsNullOrWhiteSpace(externalId))
        {
            return null;
        }

        var accountNumber = ReadString(item, "account_number")
                            ?? ReadString(item, "accountNumber")
                            ?? ReadString(item, "subAccount");
        var holderName = ReadString(item, "account_holder")
                         ?? ReadString(item, "accountHolderName")
                         ?? ReadString(item, "holder_name");
        var bankName = ReadString(item, "bank_name") ?? ReadString(item, "bankName") ?? ReadString(item, "gateway");
        var bankCode = ReadString(item, "bank_code") ?? ReadString(item, "bankCode");
        if (item.TryGetProperty("bank", out var bank) && bank.ValueKind == JsonValueKind.Object)
        {
            bankName ??= ReadString(bank, "name") ?? ReadString(bank, "short_name") ?? ReadString(bank, "code");
            bankCode ??= ReadFlexibleString(bank, "bin") ?? ReadString(bank, "code");
        }

        var name = holderName
                   ?? bankName
                   ?? (string.IsNullOrWhiteSpace(accountNumber) ? "SePay bank account" : $"SePay {ServiceTextHelper.MaskTrailing(accountNumber)}");

        return new SepayAccount
        {
            externalId = externalId,
            accountNumber = accountNumber,
            name = name,
            bankName = bankName,
            bankCode = bankCode,
            accountHolderName = holderName,
            balance = ReadDecimal(item, "balance")
                      ?? ReadDecimal(item, "current_balance")
                      ?? ReadDecimal(item, "available_balance"),
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

        var apiKey = _configuration[ConfigKeys.Sepay.ApiKey];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayCredentialMissing, ConfigKeys.Sepay.ApiKey, ErrorCodes.SepayConfigMissing);
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
        var baseUrl = (_configuration[ConfigKeys.Sepay.BaseUrl] ?? "https://bankhub-api.sepay.vn/v1").TrimEnd('/');
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
        yield return $"limit={pageSize}";
        yield return $"sort={Uri.EscapeDataString(string.IsNullOrWhiteSpace(sort) ? "asc" : sort.Trim().ToLowerInvariant())}";
        if (fromDate.HasValue)
        {
            yield return $"from_date={fromDate.Value:yyyy-MM-dd}";
        }
        if (toDate.HasValue)
        {
            yield return $"to_date={toDate.Value:yyyy-MM-dd}";
        }
    }

    private string ResolveRequired(string key, string message)
    {
        var value = _configuration[key];
        if (string.IsNullOrWhiteSpace(value))
        {
            throw AppValidationException.BadRequest(message, key, ErrorCodes.SepayConfigMissing);
        }

        return value;
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
