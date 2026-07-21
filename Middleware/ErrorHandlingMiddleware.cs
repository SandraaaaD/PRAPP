using System.Net;
using System.Text.Json;

namespace BenchApp.Middleware;

// SANDRA
// Single, central place that catches EVERY unhandled exception in the pipeline
// and turns it into the shared ApiErrorResponse contract (see ErrorContract.cs).
// No controller should ever need its own try/catch just to format an error -
// they either let exceptions bubble up, or throw ApiException for a specific case.
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (ApiException apiEx)
        {
            var response = new ApiErrorResponse
            {
                Code = apiEx.Code,
                Message = apiEx.Message,
                Retryable = apiEx.Retryable,
                FieldErrors = apiEx.FieldErrors
            };

            // Correlation id is logged server-side alongside full details for support,
            // but only the id itself goes back to the client (2.2).
            _logger.LogWarning(
                "Handled API exception. CorrelationId={CorrelationId} Code={Code} Message={Message}",
                response.CorrelationId, response.Code, apiEx.Message);

            await WriteResponse(context, apiEx.StatusCode, response);
        }
        catch (Exception ex)
        {
            var response = new ApiErrorResponse
            {
                Code = ErrorCodes.UnexpectedError,
                Message = "An unexpected error occurred. Please try again or contact support with the reference id below.",
                Retryable = true
            };

            // Full exception (with stack trace) only ever goes to server logs, never the client.
            _logger.LogError(ex,
                "Unhandled exception. CorrelationId={CorrelationId}", response.CorrelationId);

            await WriteResponse(context, (int)HttpStatusCode.InternalServerError, response);
        }
    }

    private static async Task WriteResponse(HttpContext context, int statusCode, ApiErrorResponse response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
