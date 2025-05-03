using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Common.Exceptions;
using Microsoft.Extensions.Hosting;
namespace Shared.Common.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ServiceBaseException ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var correlationId = context.Items["TraceId"] as string;

        (int statusCode, string? errorCode) = exception switch
        {
            // Custom Exceptions
            ServiceBaseException ex => (ex.StatusCode, ex.ErrorCode),

            // Standard Exceptions
            ArgumentNullException _ => ((int)HttpStatusCode.BadRequest, "ARGUMENT_NULL"),
            ArgumentOutOfRangeException _ => ((int)HttpStatusCode.BadRequest, "ARGUMENT_OUT_OF_RANGE"),
            InvalidOperationException _ => ((int)HttpStatusCode.BadRequest, "INVALID_OPERATION"),
            KeyNotFoundException _ => ((int)HttpStatusCode.NotFound, "KEY_NOT_FOUND"),
            FormatException _ => ((int)HttpStatusCode.BadRequest, "FORMAT_ERROR"),
            NullReferenceException _ => ((int)HttpStatusCode.InternalServerError, "NULL_REFERENCE"),
            UnauthorizedAccessException _ => ((int)HttpStatusCode.Forbidden, "UNAUTHORIZED_ACCESS"),
            FileNotFoundException _ => ((int)HttpStatusCode.NotFound, "FILE_NOT_FOUND"),
            IOException _ => ((int)HttpStatusCode.InternalServerError, "IO_ERROR"),
            HttpRequestException ex => ((int)HttpStatusCode.BadRequest, "BAD_HTTP_REQUEST"),
            _ => ((int)HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR")
        };

        context.Response.StatusCode = statusCode;
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (_env.IsDevelopment())
        {
            var response = new
            {
                CorrelationId = correlationId,
                StatusCode = statusCode,
                Message = exception.Message,
                ErrorCode = errorCode ?? "UNKNOWN_ERROR",
                StackTrace = exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
        else
        {
            var response = new
            {
                CorrelationId = correlationId,
                StatusCode = statusCode,
                Message = exception.Message,
                ErrorCode = errorCode ?? "UNKNOWN_ERROR",
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}