namespace BenchApp.Middleware;

// SANDRA
// Shared "problem details" style error contract used by EVERY endpoint in the app
// (Registration, Login, Profile, Reference data, Activities...).
// This is the single source of truth for how errors look on the wire.
// Acceptance criteria covered:
//  2.1 fieldErrors carries the exact field name so the frontend can map it to the right input
//  2.2 unexpected errors only ever expose correlationId + a safe message, never a stack trace
//  2.3 retryable flag tells the frontend when a retry button makes sense
//  2.4 this shape is what the contract tests (see BenchApp.Tests) assert against
public class ApiErrorResponse
{
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = "UNEXPECTED_ERROR";
    public string Message { get; set; } = "Something went wrong. Please try again.";
    public List<FieldError> FieldErrors { get; set; } = new();
    public bool Retryable { get; set; } = false;
}

public class FieldError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

// Common, named error codes so both BE and FE agree on a fixed vocabulary
// instead of guessing at free-text messages.
public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string NotFound = "NOT_FOUND";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    public const string UnexpectedError = "UNEXPECTED_ERROR";
}

// A controlled exception any controller/service can throw to produce a clean,
// predictable ApiErrorResponse instead of leaking a raw .NET exception.
public class ApiException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }
    public bool Retryable { get; }
    public List<FieldError> FieldErrors { get; }

    public ApiException(
        string code,
        string message,
        int statusCode = 400,
        bool retryable = false,
        List<FieldError>? fieldErrors = null) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
        Retryable = retryable;
        FieldErrors = fieldErrors ?? new List<FieldError>();
    }
}
