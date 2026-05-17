using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Casso;

public interface ICassoTokenProtector
{
    string Protect(CassoStoredToken token);
    CassoStoredToken Unprotect(string protectedValue);
}

public class CassoTokenProtector : ICassoTokenProtector
{
    private readonly IConfiguration _configuration;

    public CassoTokenProtector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Protect(CassoStoredToken token)
    {
        var json = JsonSerializer.Serialize(token);
        var plaintext = Encoding.UTF8.GetBytes(json);
        var key = ResolveKey();
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var ciphertext = new byte[plaintext.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        return "v1:"
               + Convert.ToBase64String(nonce)
               + ":"
               + Convert.ToBase64String(tag)
               + ":"
               + Convert.ToBase64String(ciphertext);
    }

    public CassoStoredToken Unprotect(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
        {
            throw AppValidationException.BadRequest("Casso token is missing.", "accessTokenRef", "CASSO_TOKEN_MISSING");
        }

        var parts = protectedValue.Split(':');
        if (parts.Length != 4 || parts[0] != "v1")
        {
            throw AppValidationException.BadRequest("Casso token format is invalid.", "accessTokenRef", "CASSO_TOKEN_INVALID");
        }

        var nonce = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var ciphertext = Convert.FromBase64String(parts[3]);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(ResolveKey(), tag.Length);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        var token = JsonSerializer.Deserialize<CassoStoredToken>(Encoding.UTF8.GetString(plaintext));
        if (token == null || string.IsNullOrWhiteSpace(token.accessToken))
        {
            throw AppValidationException.BadRequest("Casso token payload is invalid.", "accessTokenRef", "CASSO_TOKEN_INVALID");
        }

        return token;
    }

    private byte[] ResolveKey()
    {
        var configuredKey = _configuration["Casso:TokenEncryptionKey"]
                            ?? _configuration["JwtOptions:SecretKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw AppValidationException.BadRequest("Casso token encryption key is not configured.", "Casso:TokenEncryptionKey", "CASSO_CONFIG_MISSING");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
    }
}
