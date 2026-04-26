using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Platform.Application.Configuration;

namespace Platform.Api.Middleware;

/// <summary>Validates <c>Authorization: Bearer</c> for <c>/api/internal/v1/memory/*</c> using <see cref="MemoryWorkerOptions.ServiceToken"/>.</summary>
public sealed class InternalMemoryWorkerAuthenticationMiddleware(
    RequestDelegate next,
    IOptions<MemoryWorkerOptions> options,
    ILogger<InternalMemoryWorkerAuthenticationMiddleware> logger)
{
    private const string ItemKey = "MemoryInternalWorkerAuthenticated";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/internal/v1/memory", StringComparison.OrdinalIgnoreCase))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var configured = options.Value.ServiceToken ?? "";
        if (string.IsNullOrEmpty(configured))
        {
            logger.LogWarning("Internal memory API disabled: MemoryWorker:ServiceToken is not configured.");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Internal memory worker is not configured.", cancellationToken: context.RequestAborted).ConfigureAwait(false);
            return;
        }

        if (!TryGetBearer(context.Request, out var presented) || !FixedTimeEquals(presented, configured))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or missing service token.", cancellationToken: context.RequestAborted).ConfigureAwait(false);
            return;
        }

        context.Items[ItemKey] = true;
        await next(context).ConfigureAwait(false);
    }

    private static bool TryGetBearer(HttpRequest request, out string token)
    {
        token = "";
        if (!request.Headers.TryGetValue("Authorization", out var values))
        {
            return false;
        }

        var raw = values.ToString();
        const string prefix = "Bearer ";
        if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        token = raw[prefix.Length..].Trim();
        return token.Length > 0;
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var ab = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ab.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ab, bb);
    }
}
