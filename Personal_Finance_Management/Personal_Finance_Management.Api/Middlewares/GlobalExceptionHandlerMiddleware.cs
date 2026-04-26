using System.Text.Json;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppValidationException ex)
        {
            await WriteErrorResponse(context, ex.StatusCode, ex.Message, ex.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponse(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal server error.",
                new { code = "INTERNAL_SERVER_ERROR" });
        }
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string error,
        object details)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new
        {
            success = false,
            error,
            details,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
