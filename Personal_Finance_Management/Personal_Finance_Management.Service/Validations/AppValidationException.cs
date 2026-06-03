using Microsoft.AspNetCore.Http;
using Personal_Finance_Management.Service.Common.Constants;

namespace Personal_Finance_Management.Service.Validations;

public class AppValidationException : Exception
{
    private AppValidationException(string message, int statusCode, string code, string? field, object? details)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        Field = field;
        Details = details;
    }

    public int StatusCode { get; }
    public string Code { get; }
    public string? Field { get; }
    public object? Details { get; }

    public static AppValidationException BadRequest(string message, string field, string code)
        => new(message, StatusCodes.Status400BadRequest, code, field, null);

    public static AppValidationException BadRequest(string message, object details, string code)
        => new(message, StatusCodes.Status400BadRequest, code, null, details);

    public static AppValidationException Conflict(string message, string field, string code)
        => new(message, StatusCodes.Status409Conflict, code, field, null);

    public static AppValidationException NotFound(string message, string field, string code)
        => new(message, StatusCodes.Status404NotFound, code, field, null);

    public static AppValidationException Unauthorized(string message, string code = ErrorCodes.Unauthorized)
        => new(message, StatusCodes.Status401Unauthorized, code, null, null);

    public static AppValidationException Forbidden(string message, string code = ErrorCodes.Forbidden)
        => new(message, StatusCodes.Status403Forbidden, code, null, null);

    public static AppValidationException ValidationFailed(string message, object details)
        => new(message, StatusCodes.Status422UnprocessableEntity, ErrorCodes.ValidationFailed, null, details);
}
