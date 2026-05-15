using FluentValidation;
using Pgvector;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;

namespace Platform.Application.Features.News.Profile;

public sealed class SeedNewsProfileCommandHandler(
    IValidator<SeedNewsProfileCommand> validator,
    IUserInterestProvider interestProvider,
    INewsProfileRepository profileRepo,
    IMemoryEmbeddingGenerator embeddingGenerator)
{
    public async Task<SeedNewsProfileResult> HandleAsync(
        SeedNewsProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        // Idempotent — if the profile already exists, skip.
        if (await profileRepo.ExistsAsync(command.UserId, cancellationToken).ConfigureAwait(false))
            return SeedNewsProfileResult.Exists;

        var snapshot = await interestProvider
            .GetInterestsAsync(command.UserId, cancellationToken)
            .ConfigureAwait(false);

        var seedText = BuildSeedText(snapshot);

        var vector = await embeddingGenerator
            .TryEmbedRecallQueryAsync(seedText, cancellationToken)
            .ConfigureAwait(false);

        if (vector is null)
            return SeedNewsProfileResult.Error;

        var now = DateTimeOffset.UtcNow;
        var profile = new NewsUserProfile
        {
            UserId = command.UserId,
            LongTermEmbedding = new Vector(vector),
            SeedText = seedText,
            SeededAt = now,
            UpdatedAt = now,
        };

        await profileRepo.UpsertAsync(profile, cancellationToken).ConfigureAwait(false);
        return SeedNewsProfileResult.Seeded;
    }

    private static string BuildSeedText(UserInterestSnapshot snapshot)
    {
        var parts = new List<string>(4);

        if (snapshot.CoreInterests.Count > 0)
            parts.Add($"Core interests: {string.Join(", ", snapshot.CoreInterests)}");

        if (snapshot.SecondaryInterests.Count > 0)
            parts.Add($"Secondary interests: {string.Join(", ", snapshot.SecondaryInterests)}");

        if (snapshot.Goals.Count > 0)
            parts.Add($"Goals: {string.Join(", ", snapshot.Goals)}");

        if (snapshot.ActiveProjects.Count > 0)
            parts.Add($"Active projects: {string.Join(", ", snapshot.ActiveProjects.Select(p => p.Name))}");

        return string.Join("\n", parts);
    }
}

public enum SeedNewsProfileResult
{
    Seeded,
    Exists,
    Error,
}
