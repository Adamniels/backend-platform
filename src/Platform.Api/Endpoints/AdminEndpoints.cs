using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Platform.Api.Access;
using Platform.Api.Configuration;
using Platform.Contracts.Admin;

namespace Platform.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin");

        group.MapPost(
                "/unlock",
                async (
                    UnlockRequest body,
                    HttpContext http,
                    PlatformAccessSessionService sessions,
                    IOptions<PlatformAccessOptions> options,
                    ILoggerFactory loggerFactory,
                    CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var log = loggerFactory.CreateLogger("Platform.Access");
                    var opts = options.Value;

                    if (!sessions.IsConfigured)
                    {
                        log.LogWarning("Unlock rejected: Platform:AccessKey is not configured");
                        return Results.Problem(
                            detail: "Server access key is not configured.",
                            statusCode: StatusCodes.Status503ServiceUnavailable);
                    }

                    if (!PlatformAccessSessionService.AccessKeysEqual(opts.AccessKey, body.AccessKey))
                    {
                        log.LogWarning("Unlock failed: invalid access key");
                        return Results.Unauthorized();
                    }

                    sessions.IssueSession(http.Response);
                    log.LogInformation("Unlock succeeded");
                    return Results.Ok(new UnlockResponse(true));
                })
            .DisableAntiforgery()
            .RequireRateLimiting("unlock");

        group.MapPost(
                "/lock",
                (HttpContext http, PlatformAccessSessionService sessions) =>
                {
                    sessions.ClearSession(http.Response);
                    return Results.NoContent();
                })
            .DisableAntiforgery();

        group.MapGet(
                "/session",
                (PlatformAccessSessionService sessions, HttpContext http) =>
                    sessions.TryValidateRequest(http.Request)
                        ? Results.Ok(new SessionResponse(true))
                        : Results.Unauthorized())
            .DisableAntiforgery();

        return app;
    }

    public static void AddUnlockRateLimiter(this RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy(
            "unlock",
            httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(15),
                        QueueLimit = 0,
                    }));
    }
}
