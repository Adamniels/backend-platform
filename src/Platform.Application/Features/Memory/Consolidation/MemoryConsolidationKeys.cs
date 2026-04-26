using System.Security.Cryptography;
using System.Text;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Consolidation;

public static class MemoryConsolidationKeys
{
    public static string SemanticKeyFromEventType(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return "consolidation.event.unknown";
        }

        var chars = eventType.Trim()
            .Select(
                c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-'
                    ? c
                    : '_')
            .ToArray();
        var slug = new string(chars);
        if (slug.Length > 200)
        {
            slug = slug[..200];
        }

        return $"consolidation.event.{slug}";
    }

    public static string ProposalFingerprint(int userId, DateOnly windowEndExclusiveUtc, string eventType)
    {
        var h = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(eventType.Trim().ToLowerInvariant())));
        return $"mcons_fp_v1:u{userId}:d{windowEndExclusiveUtc:yyyyMMdd}:h{h[..16]}";
    }

    public static string? PickDomainMode(IReadOnlyList<MemoryEvent> events)
    {
        var domains = events
            .Select(e => e.Domain)
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .GroupBy(d => d!.Trim(), StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();
        return domains;
    }
}
