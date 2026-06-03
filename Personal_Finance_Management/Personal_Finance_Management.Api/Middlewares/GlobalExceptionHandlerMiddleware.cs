using System.Text.Json;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppValidationException ex)
        {
            await WriteErrorResponse(context, ex.StatusCode, ex.Code, ex.Message, ex.Field, ex.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Method} {Path}",
                context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started; global exception middleware will not write an error response");
                throw;
            }

            var (statusCode, code) = MapUnknown(ex);
            var message = statusCode >= StatusCodes.Status500InternalServerError
                ? "Internal server error."
                : ex.Message;

            await WriteErrorResponse(
                context,
                statusCode,
                code,
                message,
                field: null,
                details: _environment.IsDevelopment() ? new { detail = ex.Message } : null);
        }
    }

    private static (int statusCode, string code) MapUnknown(Exception ex) => ex switch
    {
        ArgumentException => (StatusCodes.Status400BadRequest, "BAD_REQUEST"),
        InvalidOperationException => (StatusCodes.Status400BadRequest, "BAD_REQUEST"),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "UNAUTHORIZED"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND"),
        _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR")
    };

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string code,
        string message,
        string? field,
        object? details)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new
        {
            code,
            message,
            field,
            details,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            }));
    }
}
