using Microsoft.Extensions.Logging;
using Platform.Api.Access;
using Platform.Application.Abstractions.Access;
using Platform.Application.Features.Access.UnlockSession;
using Platform.Contracts.Admin;

namespace Platform.Api.Features.Access;

public static class AdminAccessRoutes
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin");

        group.MapPost(
                "/unlock",
                async (
                    UnlockRequest body,
                    UnlockSessionCommandHandler handler,
                    HttpContext http,
                    PlatformAccessSessionService sessions,
                    ILoggerFactory loggerFactory,
                    CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var log = loggerFactory.CreateLogger("Platform.Access");

                    var outcome = await handler
                        .HandleAsync(new UnlockSessionCommand(body.AccessKey), cancellationToken)
                        .ConfigureAwait(false);

                    switch (outcome)
                    {
                        case UnlockSessionOutcome.NotConfigured:
                            log.LogWarning("Unlock rejected: Platform:AccessKey is not configured");
                            return Results.Problem(
                                detail: "Server access key is not configured.",
                                statusCode: StatusCodes.Status503ServiceUnavailable);
                        case UnlockSessionOutcome.InvalidKey:
                            log.LogWarning("Unlock failed: invalid access key");
                            return Results.Unauthorized();
                        case UnlockSessionOutcome.Success:
                        default:
                            sessions.IssueSession(http.Response);
                            log.LogInformation("Unlock succeeded");
                            return Results.Ok(new UnlockResponse(true));
                    }
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
}
