using System.Diagnostics;
using Personal_Finance_Management.Service.Base;
using Serilog.Context;

namespace Personal_Finance_Management.Api.Middlewares;

public class CorrelationIdMiddleware
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentUserAccessor currentUser)
    {
        var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Activity.Current?.Id ?? context.TraceIdentifier;
        }

        context.Response.Headers[CorrelationHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId",
                   currentUser.TryGetUserId(out var userId) ? userId.ToString() : "anonymous"))
        {
            await _next(context);
        }
    }
}
