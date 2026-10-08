using System.Diagnostics;
using System.Net;
using EnterpriseIam.WebApi.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace EnterpriseIam.WebApi;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    // Primary constructor tracks core logging infrastructure
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the failure securely on the backend server terminal console
        _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // 2. Classify the HTTP Status Code mapping based on the specific type of crash
        var statusCode = exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException or InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError // Catch-all for unexpected infrastructure drops
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        // 3. Mask descriptive data if deployed to Production to shield from bad actors
        var details = _env.IsDevelopment()
            ? exception.ToString()
            : "Please contact support with the timestamp signature for administrative resolution tracking.";

        var response = new ErrorResponse(
            StatusCode: statusCode,
            Message: exception.Message,
            Details: details,
            TimestampUtc: DateTime.UtcNow
        );

        var options = new System.Text.Json.JsonSerializerOptions
        {
            TypeInfoResolver = AppJsonContext.Default
        };

        // 4. Stream the clean JSON object payload straight down to the client container response window
        await httpContext.Response.WriteAsJsonAsync(response, options, cancellationToken);

        // Return true to tell the .NET runtime that this exception has been safely resolved
        return true;
    }
}