using FluentValidation;
using Pgvector;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;

namespace Platform.Application.Features.News.Profile;

public sealed class UpdateNewsProfileCommandHandler(
    IValidator<UpdateNewsProfileCommand> validator,
    INewsInteractionRepository interactions,
    INewsEmbeddingRepository embeddings,
    INewsProfileRepository profiles,
    IMemoryEmbeddingGenerator embeddingGenerator)
{
    // Momentum factor for long-term — how much of the existing profile is preserved each update.
    // At 0.85 the profile requires ~13 runs to shift significantly (half-life ~6 runs).
    private const double Alpha = 0.85;

    // Long-term interaction window in days.
    private const int LongTermWindowDays = 7;

    // Short-term interaction window in days — raw snapshot, no momentum.
    private const int ShortTermWindowDays = 14;

    // Minimum weight for any read interaction regardless of dwell time.
    private const double ReadWeightMin = 0.2;

    // Maximum additional weight gained from a 2× average dwell time.
    private const double ReadWeightRange = 0.6;

    // Weight applied to a save interaction.
    private const double SaveWeight = 1.0;

    // Weight applied to a dismiss (negative — pulls profile away from the article topic).
    private const double DismissWeight = -0.3;

    // Default average dwell (seconds) used when the user has no prior read history.
    private const double DefaultAverageDwellSeconds = 60.0;

    public async Task<UpdateNewsProfileResult> HandleAsync(
        UpdateNewsProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        // Guard: profile must exist before we can update it.
        var profile = await profiles
            .GetAsync(command.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
            return UpdateNewsProfileResult.NoProfile;

        // Fetch the wider 14-day window in one query — it is a superset of the 7-day window,
        // so both long-term and short-term can be computed from a single DB round-trip.
        var since14 = DateTimeOffset.UtcNow.AddDays(-ShortTermWindowDays);
        var all14 = await interactions
            .GetRecentAsync(command.UserId, since14, cancellationToken)
            .ConfigureAwait(false);

        if (all14.Count == 0)
            return UpdateNewsProfileResult.NoData;

        // Load embeddings for all interacted articles in one query.
        var interactedIds = all14.Select(i => i.NewsItemId).Distinct().ToArray();
        var stored = await embeddings
            .GetByNewsItemIdsAsync(interactedIds, embeddingGenerator.ModelKey, cancellationToken)
            .ConfigureAwait(false);

        if (stored.Count == 0)
            return UpdateNewsProfileResult.NoData;

        var embeddingMap = stored.ToDictionary(e => e.NewsItemId, e => e.Embedding.ToArray());

        // Compute dwell baseline for normalization across both windows.
        var avgDwell = await interactions
            .GetAverageDwellSecondsAsync(command.UserId, cancellationToken)
            .ConfigureAwait(false) ?? DefaultAverageDwellSeconds;

        var dimensions = embeddingGenerator.Dimensions;
        var now = DateTimeOffset.UtcNow;

        // ── Long-term: 7-day subset, blended with momentum ───────────────────────
        var cutoff7 = now.AddDays(-LongTermWindowDays);
        var recent7 = all14.Where(i => i.RecordedAt >= cutoff7).ToList();
        var longSignal = ComputeWeightedSum(recent7, embeddingMap, avgDwell, dimensions);
        if (longSignal is not null)
        {
            var currentVec = profile.LongTermEmbedding.ToArray();
            var blended = new double[dimensions];
            for (var i = 0; i < dimensions; i++)
                blended[i] = Alpha * currentVec[i] + (1.0 - Alpha) * longSignal[i];

            var longResult = Normalize(blended);
            if (longResult is not null)
            {
                profile.LongTermEmbedding = new Vector(longResult.Select(d => (float)d).ToArray());
                profile.UpdatedAt = now;
            }
        }

        // ── Short-term: full 14-day window, raw weighted average, no momentum ────
        var shortSignal = ComputeWeightedSum(all14, embeddingMap, avgDwell, dimensions);
        if (shortSignal is not null)
        {
            profile.ShortTermEmbedding = new Vector(shortSignal.Select(d => (float)d).ToArray());
            profile.ShortTermUpdatedAt = now;
        }

        var anyUpdate = longSignal is not null || shortSignal is not null;
        if (!anyUpdate)
            return UpdateNewsProfileResult.NoData;

        await profiles.UpsertAsync(profile, cancellationToken).ConfigureAwait(false);
        return UpdateNewsProfileResult.Updated;
    }

    /// <summary>
    /// Computes the normalized weighted sum of article embeddings for the given interactions.
    /// Returns null when there is no signal (no matched embeddings, or all weights cancel to zero).
    /// </summary>
    private static double[]? ComputeWeightedSum(
        IEnumerable<NewsInteraction> source,
        Dictionary<string, float[]> embeddingMap,
        double avgDwell,
        int dimensions)
    {
        var weightedSum = new double[dimensions];
        var hasSignal = false;

        foreach (var interaction in source)
        {
            if (!embeddingMap.TryGetValue(interaction.NewsItemId, out var vec))
                continue;  // article ingested before Phase 2 — no embedding, skip gracefully

            var weight = ComputeWeight(interaction, avgDwell);
            for (var i = 0; i < dimensions; i++)
                weightedSum[i] += weight * vec[i];

            hasSignal = true;
        }

        return hasSignal ? Normalize(weightedSum) : null;
    }

    private static double ComputeWeight(NewsInteraction interaction, double avgDwell)
    {
        return interaction.Type switch
        {
            NewsInteractionType.Save    => SaveWeight,
            NewsInteractionType.Dismiss => DismissWeight,
            NewsInteractionType.Read    => ComputeReadWeight(interaction.DwellSeconds ?? 0, avgDwell),
            _                           => 0.0,
        };
    }

    private static double ComputeReadWeight(int dwellSeconds, double avgDwell)
    {
        // dwell_factor ∈ [0.0, 1.0]: clamp(dwell / avg, 0, 2) / 2
        var dwellFactor = Math.Clamp((double)dwellSeconds / avgDwell, 0.0, 2.0) / 2.0;
        return ReadWeightMin + ReadWeightRange * dwellFactor;
    }

    private static double[]? Normalize(double[] v)
    {
        var magnitude = Math.Sqrt(v.Sum(x => x * x));
        if (magnitude < 1e-10)
            return null;

        var result = new double[v.Length];
        for (var i = 0; i < v.Length; i++)
            result[i] = v[i] / magnitude;

        return result;
    }
}
