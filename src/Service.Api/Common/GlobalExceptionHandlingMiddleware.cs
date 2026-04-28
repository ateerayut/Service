using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Service.Api.Common;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
                throw;

            var problemDetails = CreateProblemDetails(context, exception);

            _logger.LogError(
                exception,
                "Unhandled exception. TraceId: {TraceId}, CorrelationId: {CorrelationId}",
                context.TraceIdentifier,
                context.GetCorrelationId());

            context.Response.Clear();
            context.Response.StatusCode = problemDetails.Status
                ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problemDetails,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                context.RequestAborted);
        }
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Detail = _environment.IsDevelopment()
                ? exception.ToString()
                : null,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        var correlationId = context.GetCorrelationId();

        if (!string.IsNullOrWhiteSpace(correlationId))
            problemDetails.Extensions["correlationId"] = correlationId;

        return problemDetails;
    }
}
