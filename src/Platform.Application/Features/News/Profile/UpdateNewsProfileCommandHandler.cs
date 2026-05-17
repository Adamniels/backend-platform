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
    // Momentum factor — how much of the existing profile is preserved each update.
    // At 0.85 the profile requires ~13 runs to shift significantly (half-life ~6 runs).
    private const double Alpha = 0.85;

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

        var since = DateTimeOffset.UtcNow.AddDays(-command.WindowDays);
        var recent = await interactions
            .GetRecentAsync(command.UserId, since, cancellationToken)
            .ConfigureAwait(false);

        if (recent.Count == 0)
            return UpdateNewsProfileResult.NoData;

        // Load embeddings for all interacted articles in one query.
        var interactedIds = recent.Select(i => i.NewsItemId).Distinct().ToArray();
        var stored = await embeddings
            .GetByNewsItemIdsAsync(interactedIds, embeddingGenerator.ModelKey, cancellationToken)
            .ConfigureAwait(false);

        if (stored.Count == 0)
            return UpdateNewsProfileResult.NoData;

        var embeddingMap = stored.ToDictionary(e => e.NewsItemId, e => e.Embedding.ToArray());

        // Compute dwell baseline for normalization.
        var avgDwell = await interactions
            .GetAverageDwellSecondsAsync(command.UserId, cancellationToken)
            .ConfigureAwait(false) ?? DefaultAverageDwellSeconds;

        // Accumulate the weighted sum across all interactions that have embeddings.
        var dimensions = embeddingGenerator.Dimensions;
        var weightedSum = new double[dimensions];
        var hasSignal = false;

        foreach (var interaction in recent)
        {
            if (!embeddingMap.TryGetValue(interaction.NewsItemId, out var vec))
                continue;  // article was ingested before Phase 2 — skip gracefully

            var weight = ComputeWeight(interaction, avgDwell);

            for (var i = 0; i < dimensions; i++)
                weightedSum[i] += weight * vec[i];

            hasSignal = true;
        }

        if (!hasSignal)
            return UpdateNewsProfileResult.NoData;

        var signal = Normalize(weightedSum);
        if (signal is null)
            return UpdateNewsProfileResult.NoData;  // all weights cancelled to zero

        // Blend: new = normalize(alpha * current + (1 - alpha) * signal)
        var currentVec = profile.LongTermEmbedding.ToArray();
        var blended = new double[dimensions];
        for (var i = 0; i < dimensions; i++)
            blended[i] = Alpha * currentVec[i] + (1.0 - Alpha) * signal[i];

        var result = Normalize(blended);
        if (result is null)
            return UpdateNewsProfileResult.Error;

        profile.LongTermEmbedding = new Vector(result.Select(d => (float)d).ToArray());
        profile.UpdatedAt = DateTimeOffset.UtcNow;

        await profiles.UpsertAsync(profile, cancellationToken).ConfigureAwait(false);
        return UpdateNewsProfileResult.Updated;
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
