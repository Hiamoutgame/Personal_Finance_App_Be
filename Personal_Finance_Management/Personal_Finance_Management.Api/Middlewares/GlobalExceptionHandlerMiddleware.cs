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
            await WriteErrorResponse(context, ex.StatusCode, ex.Message, ex.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred while processing request {Path}", context.Request.Path);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("The response has already started, the global exception middleware will not write an error response");
                throw;
            }

            var statusCode = MapStatusCode(ex);
            var message = ResolveClientMessage(ex, statusCode);

            await WriteErrorResponse(
                context,
                statusCode,
                message,
                _environment.IsDevelopment() ? new { detail = ex.Message } : null);
        }
    }

    private static int MapStatusCode(Exception ex)
    {
        return ex switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string ResolveClientMessage(Exception ex, int statusCode)
    {
        return statusCode >= StatusCodes.Status500InternalServerError
            ? "Internal server error."
            : ex.Message;
    }

    private static async Task WriteErrorResponse(
        HttpContext context,
        int statusCode,
        string error,
        object? details)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var body = new
        {
            success = false,
            error,
            message = error,
            details,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
