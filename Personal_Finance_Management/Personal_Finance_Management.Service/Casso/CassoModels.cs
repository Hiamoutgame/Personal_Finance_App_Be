using System.Text.Json;

namespace Personal_Finance_Management.Service.Casso;

public class CassoTokenResponse
{
    public string accessToken { get; set; } = null!;
    public string? refreshToken { get; set; }
    public string tokenType { get; set; } = "bearer";
    public int? expiresIn { get; set; }
    public DateTimeOffset? expiresAt { get; set; }
}

public class CassoStoredToken
{
    public string accessToken { get; set; } = null!;
    public string? refreshToken { get; set; }
    public string tokenType { get; set; } = "bearer";
    public DateTimeOffset? expiresAt { get; set; }
}

public class CassoAccount
{
    public string externalId { get; set; } = null!;
    public string? accountNumber { get; set; }
    public string name { get; set; } = "Casso bank account";
    public string? bankName { get; set; }
    public string? bankCode { get; set; }
    public string? accountHolderName { get; set; }
    public decimal? balance { get; set; }
    public string rawJson { get; set; } = "{}";
}

public class CassoTransactionRecord
{
    public JsonElement payload { get; set; }
}
