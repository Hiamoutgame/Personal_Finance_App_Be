using FluentValidation;

namespace Personal_Finance_Management.Service.Validations;

public static class FluentValidationExtensions
{
    public static async Task ValidateOrThrowAppException<T>(
        this IValidator<T> validator,
        T request)
    {
        if (request is null)
        {
            throw AppValidationException.BadRequest(
                "Request body is required.",
                "body",
                "REQUIRED");
        }

        var result = await validator.ValidateAsync(request);
        if (result.IsValid)
        {
            return;
        }

        var errors = result.Errors
            .Select(error => new
            {
                field = ToCamelCase(error.PropertyName),
                code = string.IsNullOrWhiteSpace(error.ErrorCode)
                    ? "INVALID"
                    : error.ErrorCode,
                error = error.ErrorMessage
            })
            .ToArray();

        throw AppValidationException.BadRequest(
            "Invalid request data.",
            errors,
            "REQUEST_INVALID");
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
