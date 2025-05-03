using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Shared.Common.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the request already has a Correlation ID
        if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
        {
            // Generate a new Correlation ID if it doesn't exist
            correlationId = Guid.NewGuid().ToString();
            context.Request.Headers["X-Correlation-ID"] = correlationId;
        }

        // Set the correlation ID in the response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            return Task.CompletedTask;
        });

        context.Items["TraceId"] = correlationId;

        
        await _next(context);
    }
}
