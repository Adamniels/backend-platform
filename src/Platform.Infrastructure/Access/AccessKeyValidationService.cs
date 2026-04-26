using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.Access;
using Platform.Application.Configuration;

namespace Platform.Infrastructure.Access;

public sealed class AccessKeyValidationService(IOptions<PlatformAccessOptions> options) : IAccessKeyValidationService
{
    public UnlockSessionOutcome ValidateAccessKey(string? accessKey)
    {
        var configured = options.Value.AccessKey;
        if (string.IsNullOrEmpty(configured))
        {
            return UnlockSessionOutcome.NotConfigured;
        }

        return AccessKeysEqual(configured, accessKey)
            ? UnlockSessionOutcome.Success
            : UnlockSessionOutcome.InvalidKey;
    }

    private static bool AccessKeysEqual(string configured, string? provided)
    {
        if (string.IsNullOrEmpty(configured) || provided is null)
        {
            return false;
        }

        var ah = SHA256.HashData(Encoding.UTF8.GetBytes(configured));
        var bh = SHA256.HashData(Encoding.UTF8.GetBytes(provided));
        return CryptographicOperations.FixedTimeEquals(ah, bh);
    }
}
