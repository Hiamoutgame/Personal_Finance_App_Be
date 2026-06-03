using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Personal_Finance_Management.Service.Common.Constants;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Sepay;

public interface ISepayTokenProtector
{
    string Protect(SepayStoredToken token);
    SepayStoredToken Unprotect(string protectedValue);
}

public class SepayTokenProtector : ISepayTokenProtector
{
    private readonly IConfiguration _configuration;

    public SepayTokenProtector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Protect(SepayStoredToken token)
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

    public SepayStoredToken Unprotect(string protectedValue)
    {
        if (string.IsNullOrWhiteSpace(protectedValue))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayTokenMissing, "accessTokenRef", ErrorCodes.SepayTokenMissing);
        }

        var parts = protectedValue.Split(':');
        if (parts.Length != 4 || parts[0] != "v1")
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayTokenFormatInvalid, "accessTokenRef", ErrorCodes.SepayTokenInvalid);
        }

        var nonce = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var ciphertext = Convert.FromBase64String(parts[3]);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(ResolveKey(), tag.Length);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        var token = JsonSerializer.Deserialize<SepayStoredToken>(Encoding.UTF8.GetString(plaintext));
        if (token == null || string.IsNullOrWhiteSpace(token.accessToken))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayTokenPayloadInvalid, "accessTokenRef", ErrorCodes.SepayTokenInvalid);
        }

        return token;
    }

    private byte[] ResolveKey()
    {
        var configuredKey = _configuration[ConfigKeys.Sepay.TokenEncryptionKey]
                            ?? _configuration["JwtOptions:SecretKey"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            throw AppValidationException.BadRequest(ErrorMessages.SepayTokenEncryptionKeyMissing, ConfigKeys.Sepay.TokenEncryptionKey, ErrorCodes.SepayConfigMissing);
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
    }
}
