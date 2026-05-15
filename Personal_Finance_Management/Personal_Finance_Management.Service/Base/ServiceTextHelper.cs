using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.Base;

public static class ServiceTextHelper
{
    public static string NormalizeRequiredText(string? value, string field, string message)
    {
        var normalizedValue = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            throw AppValidationException.BadRequest(message, field, "REQUIRED");
        }

        return normalizedValue;
    }

    public static void ValidateRequiredText(string? value, string field, string message)
    {
        NormalizeRequiredText(value, field, message);
    }

    public static string? NormalizeOptionalText(string? value)
    {
        var normalizedValue = value?.Trim();
        return string.IsNullOrWhiteSpace(normalizedValue) ? null : normalizedValue;
    }

    public static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }

    public static string MaskSecret(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmedValue = value.Trim();
        if (trimmedValue.Length <= 8)
        {
            return "****";
        }

        return $"{trimmedValue[..4]}...{trimmedValue[^4..]}";
    }

    public static string? MaskOptionalSecret(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : MaskSecret(value);
    }

    public static string MaskTrailing(string? value, int visibleTrailingLength = 4)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmedValue = value.Trim();
        if (trimmedValue.Length <= visibleTrailingLength)
        {
            return trimmedValue;
        }

        return new string('*', trimmedValue.Length - visibleTrailingLength)
               + trimmedValue[^visibleTrailingLength..];
    }

    public static string NormalizeEnum<TEnum>(string value)
        where TEnum : struct, Enum
    {
        return Enum.Parse<TEnum>(value.Trim(), ignoreCase: true).ToString();
    }
}
