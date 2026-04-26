using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Platform.Api.Access;
using Platform.Application.Configuration;

namespace Platform.Api.Middleware;

public sealed class RequirePlatformAccessMiddleware(RequestDelegate next, ILogger<RequirePlatformAccessMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        PlatformAccessSessionService sessions,
        IOptions<PlatformAccessOptions> options,
        IWebHostEnvironment environment)
    {
        if (HttpMethods.IsOptions(context.Request.Method))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var path = context.Request.Path;
        if (ShouldBypass(path, context, options.Value, environment))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (sessions.TryValidateRequest(context.Request))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        logger.LogInformation(
            "Access denied: missing or invalid session. {Method} {Path}",
            context.Request.Method,
            path);
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private static bool ShouldBypass(
        PathString path,
        HttpContext context,
        PlatformAccessOptions opts,
        IWebHostEnvironment environment)
    {
        if (path.Equals("/api/admin/unlock", StringComparison.OrdinalIgnoreCase)
            && HttpMethods.IsPost(context.Request.Method))
        {
            return true;
        }

        if (path.Equals("/api/admin/lock", StringComparison.OrdinalIgnoreCase)
            && HttpMethods.IsPost(context.Request.Method))
        {
            return true;
        }

        if (opts.PublicHealth
            && (path.Equals("/health", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/ready", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        if (environment.IsDevelopment() && path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
