using Microsoft.AspNetCore.Builder;
using Shared.Common.Middleware;

namespace Shared.Common.Extensions;

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }
}