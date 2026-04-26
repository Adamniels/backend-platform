using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Platform.Api.Features.Access;

public static class AccessRateLimiting
{
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
