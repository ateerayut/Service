using Serilog.Context;

namespace Service.Api.Common;

public static class CorrelationIdHttpContextExtensions
{
    public const string HeaderName = "X-Correlation-Id";

    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items.TryGetValue(HeaderName, out var value)
            ? value as string
            : null;
    }
}

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        context.Items[CorrelationIdHttpContextExtensions.HeaderName] = correlationId;
        context.Response.Headers[CorrelationIdHttpContextExtensions.HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var headerValue = context.Request.Headers[
            CorrelationIdHttpContextExtensions.HeaderName].FirstOrDefault();

        return string.IsNullOrWhiteSpace(headerValue)
            ? Guid.CreateVersion7().ToString()
            : headerValue;
    }
}
