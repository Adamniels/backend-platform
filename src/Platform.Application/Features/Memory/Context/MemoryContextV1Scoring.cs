using Platform.Domain.Features.Memory;

namespace Platform.Application.Features.Memory.Context;

/// <summary>Deterministic scoring for v1 memory context (no ML). Heavily weights explicit user truth over inferred rows.</summary>
public static class MemoryContextV1Scoring
{
    public const double ExplicitProfileRankFloor = 0.85d;

    /// <summary>Weights for combined rank (sum = 1.0).</summary>
    public const double WAuthority = 0.38d;
    public const double WConfidence = 0.22d;
    public const double WRecency = 0.18d;
    public const double WWorkflow = 0.12d;
    public const double WProject = 0.10d;

    public static IReadOnlyList<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<string>();
        }

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var word in text.Split(
                     new[] { ' ', '\t', '\n', '\r', ',', '.', ';', ':', '/', '-', '_', '"', '\'', '(', ')' },
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (word.Length < 2)
            {
                continue;
            }

            set.Add(word.ToLowerInvariant());
        }

        return set.ToList();
    }

    /// <summary>0..1 where 1 is now and 0 is infinitely old. Uses half-life in days (exponential decay).</summary>
    public static double RecencyScore(DateTimeOffset at, DateTimeOffset now, double halfLifeDays = 30d)
    {
        if (halfLifeDays <= 0d)
        {
            return 0.5d;
        }

        var days = (now - at).TotalDays;
        if (days <= 0d)
        {
            return 1d;
        }

        return Math.Exp(-Math.Log(2d) * days / halfLifeDays);
    }

    public static double TextMatchRatio(IReadOnlyList<string> queryTokens, params string?[] fields)
    {
        if (queryTokens.Count == 0)
        {
            return 0.5d;
        }

        var text = string.Join(" ", fields.Where(s => !string.IsNullOrEmpty(s)));
        if (string.IsNullOrEmpty(text))
        {
            return 0d;
        }

        var lower = text.ToLowerInvariant();
        var hit = 0;
        foreach (var t in queryTokens)
        {
            if (lower.Contains(t, StringComparison.Ordinal))
            {
                hit++;
            }
        }

        return (double)hit / queryTokens.Count;
    }

    public static double WorkflowRelevance(string? requestWorkflow, string? entityWorkflow)
    {
        if (string.IsNullOrWhiteSpace(requestWorkflow) || string.IsNullOrWhiteSpace(entityWorkflow))
        {
            return 0.4d;
        }

        if (string.Equals(
                requestWorkflow.Trim(),
                entityWorkflow.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            return 1d;
        }

        if (entityWorkflow.Contains(requestWorkflow, StringComparison.OrdinalIgnoreCase) ||
            requestWorkflow.Contains(entityWorkflow, StringComparison.OrdinalIgnoreCase))
        {
            return 0.7d;
        }

        return 0.2d;
    }

    public static double ProjectRelevance(string? requestProject, string? entityProject)
    {
        if (string.IsNullOrWhiteSpace(requestProject) || string.IsNullOrWhiteSpace(entityProject))
        {
            return 0.35d;
        }

        return string.Equals(
            requestProject.Trim(),
            entityProject.Trim(),
            StringComparison.OrdinalIgnoreCase)
            ? 1d
            : 0.2d;
    }

    public static double SemanticStatusFactor(SemanticMemoryStatus status) =>
        status switch
        {
            SemanticMemoryStatus.Active => 1d,
            SemanticMemoryStatus.PendingReview => 0.72d,
            SemanticMemoryStatus.Unknown => 0.5d,
            SemanticMemoryStatus.Rejected => 0.15d,
            _ => 0.2d,
        };

    public static string SemanticStatusString(SemanticMemoryStatus s) => s switch
    {
        SemanticMemoryStatus.Active => "Active",
        SemanticMemoryStatus.PendingReview => "PendingReview",
        SemanticMemoryStatus.Superseded => "Superseded",
        SemanticMemoryStatus.Archived => "Archived",
        SemanticMemoryStatus.Rejected => "Rejected",
        _ => s.ToString(),
    };

    public static string ProceduralStatusString(ProceduralRuleStatus s) => s switch
    {
        ProceduralRuleStatus.Active => "Active",
        ProceduralRuleStatus.Inactive => "Inactive",
        ProceduralRuleStatus.Deprecated => "Deprecated",
        _ => s.ToString(),
    };

    /// <summary>Primary rank for non-explicit rows (semantic, events, rules). domainMatch is 0..1 for request domain vs row domain.</summary>
    public static double CombinedLearnerRank(
        double authority,
        double confidence,
        double recency,
        double workflowRel,
        double projectRel,
        double textMatch,
        double domainMatch,
        double statusFactor)
    {
        var baseScore =
            WAuthority * authority +
            WConfidence * confidence +
            WRecency * recency +
            WWorkflow * workflowRel +
            WProject * projectRel;
        return baseScore * (0.55d + 0.45d * textMatch) * (0.65d + 0.35d * domainMatch) * statusFactor;
    }

    /// <summary>Guarantees explicit profile slices stay above typical inferred rows when other signals are similar.</summary>
    public static double ExplicitProfileItemRank(double raw) =>
        Math.Max(raw, ExplicitProfileRankFloor);
}
