using FluentValidation;
using Pgvector;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;

namespace Platform.Application.Features.News.Profile;

public sealed class UpdateNewsActiveContextCommandHandler(
    IValidator<UpdateNewsActiveContextCommand> validator,
    IUserInterestProvider interestProvider,
    INewsProfileRepository profiles,
    IMemoryEmbeddingGenerator embeddingGenerator)
{
    public async Task<UpdateNewsActiveContextResult> HandleAsync(
        UpdateNewsActiveContextCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var profile = await profiles
            .GetAsync(command.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
            return UpdateNewsActiveContextResult.NoProfile;

        var snapshot = await interestProvider
            .GetInterestsAsync(command.UserId, cancellationToken)
            .ConfigureAwait(false);

        var text = BuildContextText(snapshot);

        var vector = await embeddingGenerator
            .TryEmbedRecallQueryAsync(text, cancellationToken)
            .ConfigureAwait(false);

        if (vector is null)
            return UpdateNewsActiveContextResult.Error;

        profile.ActiveContextEmbedding = new Vector(vector);
        profile.ActiveContextUpdatedAt = DateTimeOffset.UtcNow;

        await profiles.UpsertAsync(profile, cancellationToken).ConfigureAwait(false);
        return UpdateNewsActiveContextResult.Updated;
    }

    // Same text structure used when seeding the initial profile — keeps the active context
    // embedding in the same semantic space as the long-term seed so cosine distances are comparable.
    private static string BuildContextText(UserInterestSnapshot snapshot)
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
