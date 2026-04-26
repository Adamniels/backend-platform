using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Platform.Application.Configuration;

namespace Platform.Api.Access;

public sealed class PlatformAccessSessionService(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<PlatformAccessOptions> options,
    ILogger<PlatformAccessSessionService> logger)
{
    private const string ProtectorPurpose = "Platform.AccessSession.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private IDataProtector Protector => dataProtectionProvider.CreateProtector(ProtectorPurpose);

    public bool IsConfigured => !string.IsNullOrEmpty(options.Value.AccessKey);

    public bool TryValidateRequest(HttpRequest request)
    {
        var opts = options.Value;
        if (!request.Cookies.TryGetValue(opts.CookieName, out var raw) || string.IsNullOrEmpty(raw))
        {
            return false;
        }

        try
        {
            var bytes = Convert.FromBase64String(raw);
            var unprotected = Protector.Unprotect(bytes);
            var ticket = JsonSerializer.Deserialize<AccessSessionTicket>(unprotected, JsonOptions);
            if (ticket is null || string.IsNullOrEmpty(ticket.Nonce))
            {
                return false;
            }

            if (ticket.ExpiresAtUtc <= DateTimeOffset.UtcNow)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Invalid or tampered access session cookie");
            return false;
        }
    }

    public void IssueSession(HttpResponse response)
    {
        var opts = options.Value;
        var expires = DateTimeOffset.UtcNow.AddHours(Math.Clamp(opts.SessionHours, 1, 168));
        var ticket = new AccessSessionTicket(expires, Guid.NewGuid().ToString("N"));
        var payload = JsonSerializer.SerializeToUtf8Bytes(ticket, JsonOptions);
        var protectedBytes = Protector.Protect(payload);
        var cookieValue = Convert.ToBase64String(protectedBytes);

        response.Cookies.Append(
            opts.CookieName,
            cookieValue,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = opts.CookieSecure,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromHours(Math.Clamp(opts.SessionHours, 1, 168)),
                Path = "/",
                IsEssential = true,
            });
    }

    public void ClearSession(HttpResponse response)
    {
        var opts = options.Value;
        response.Cookies.Delete(
            opts.CookieName,
            new CookieOptions
            {
                Path = "/",
                Secure = opts.CookieSecure,
                SameSite = SameSiteMode.Strict,
            });
    }

    public static bool AccessKeysEqual(string? configured, string? provided)
    {
        if (string.IsNullOrEmpty(configured) || provided is null)
        {
            return false;
        }

        var ah = SHA256.HashData(Encoding.UTF8.GetBytes(configured));
        var bh = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
        return CryptographicOperations.FixedTimeEquals(ah, bh);
    }

    private sealed record AccessSessionTicket(DateTimeOffset ExpiresAtUtc, string Nonce);
}
