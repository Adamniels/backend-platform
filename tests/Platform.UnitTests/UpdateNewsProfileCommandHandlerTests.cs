using FluentValidation;
using Pgvector;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Application.Features.News.Profile;
using Platform.Domain.Features.News;

namespace Platform.UnitTests;

public sealed class UpdateNewsProfileCommandHandlerTests
{
    // ---------------------------------------------------------------------------
    // Shared helpers
    // ---------------------------------------------------------------------------

    private static readonly string ArticleId = "ni-" + new string('c', 32);

    private static float[] UnitVec(int dims, int nonZeroIndex = 0)
    {
        var v = new float[dims];
        v[nonZeroIndex] = 1.0f;
        return v;
    }

    private static NewsUserProfile MakeProfile(int dims = 3) =>
        new()
        {
            UserId = 1,
            LongTermEmbedding = new Vector(UnitVec(dims)),
            SeedText = "seed",
            SeededAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

    private static NewsInteraction MakeInteraction(
        NewsInteractionType type,
        int? dwellSeconds = null,
        string? itemId = null) =>
        new()
        {
            UserId = 1,
            NewsItemId = itemId ?? ArticleId,
            Type = type,
            DwellSeconds = dwellSeconds,
            RecordedAt = DateTimeOffset.UtcNow,
        };

    private static NewsItemEmbedding MakeEmbedding(float[] vec, string? itemId = null) =>
        new()
        {
            NewsItemId = itemId ?? ArticleId,
            EmbeddingModelKey = "stub-v1",
            Dimensions = vec.Length,
            Embedding = new Vector(vec),
            EmbeddedAt = DateTimeOffset.UtcNow,
        };

    // ---------------------------------------------------------------------------
    // Test doubles
    // ---------------------------------------------------------------------------

    private sealed class StubInteractionRepository(
        IReadOnlyList<NewsInteraction>? recent = null,
        double? avgDwell = null) : INewsInteractionRepository
    {
        public Task InsertAsync(NewsInteraction interaction, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<NewsInteraction>> GetRecentAsync(
            int userId,
            DateTimeOffset since,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(recent ?? (IReadOnlyList<NewsInteraction>)[]);

        public Task<double?> GetAverageDwellSecondsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(avgDwell);
    }

    private sealed class StubEmbeddingRepository(IReadOnlyList<NewsItemEmbedding>? stored = null) : INewsEmbeddingRepository
    {
        public Task<bool> ExistsAsync(string newsItemId, string modelKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task UpsertAsync(NewsItemEmbedding embedding, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<NewsItemEmbedding>> GetByNewsItemIdsAsync(
            IEnumerable<string> newsItemIds,
            string modelKey,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(stored ?? (IReadOnlyList<NewsItemEmbedding>)[]);
    }

    private sealed class CapturingProfileRepository(NewsUserProfile? profile = null) : INewsProfileRepository
    {
        public NewsUserProfile? Upserted { get; private set; }

        public Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(profile is not null);

        public Task<NewsUserProfile?> GetAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(profile);

        public Task UpsertAsync(NewsUserProfile p, CancellationToken cancellationToken = default)
        {
            Upserted = p;
            return Task.CompletedTask;
        }
    }

    private sealed class StubEmbeddingGenerator(int dims = 3) : IMemoryEmbeddingGenerator
    {
        public string ModelKey => "stub-v1";
        public int Dimensions => dims;

        public Task<float[]?> TryEmbedRecallQueryAsync(string? text, CancellationToken cancellationToken = default) =>
            Task.FromResult<float[]?>(UnitVec(dims));
    }

    private static UpdateNewsProfileCommandHandler MakeHandler(
        NewsUserProfile? profile = null,
        IReadOnlyList<NewsInteraction>? interactions = null,
        IReadOnlyList<NewsItemEmbedding>? embeddings = null,
        double? avgDwell = null,
        int dims = 3) =>
        new(
            new UpdateNewsProfileCommandValidator(),
            new StubInteractionRepository(interactions, avgDwell),
            new StubEmbeddingRepository(embeddings),
            new CapturingProfileRepository(profile),
            new StubEmbeddingGenerator(dims));

    private static UpdateNewsProfileCommandHandler MakeHandlerWithCapturingProfile(
        out CapturingProfileRepository profileRepo,
        NewsUserProfile? profile = null,
        IReadOnlyList<NewsInteraction>? interactions = null,
        IReadOnlyList<NewsItemEmbedding>? embeddings = null,
        double? avgDwell = null,
        int dims = 3)
    {
        profileRepo = new CapturingProfileRepository(profile);
        return new UpdateNewsProfileCommandHandler(
            new UpdateNewsProfileCommandValidator(),
            new StubInteractionRepository(interactions, avgDwell),
            new StubEmbeddingRepository(embeddings),
            profileRepo,
            new StubEmbeddingGenerator(dims));
    }

    private static readonly UpdateNewsProfileCommand ValidCommand = new(1, 7);

    // ---------------------------------------------------------------------------
    // Result-path tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Returns_NoProfile_when_profile_does_not_exist()
    {
        var handler = MakeHandler(profile: null);
        var result = await handler.HandleAsync(ValidCommand);
        Assert.Equal(UpdateNewsProfileResult.NoProfile, result);
    }

    [Fact]
    public async Task Returns_NoData_when_no_recent_interactions()
    {
        var handler = MakeHandler(profile: MakeProfile(), interactions: []);
        var result = await handler.HandleAsync(ValidCommand);
        Assert.Equal(UpdateNewsProfileResult.NoData, result);
    }

    [Fact]
    public async Task Returns_NoData_when_interactions_have_no_matching_embeddings()
    {
        var interactions = new[] { MakeInteraction(NewsInteractionType.Save) };
        // Embeddings list is empty — no embedding for the interacted article.
        var handler = MakeHandler(
            profile: MakeProfile(),
            interactions: interactions,
            embeddings: []);
        var result = await handler.HandleAsync(ValidCommand);
        Assert.Equal(UpdateNewsProfileResult.NoData, result);
    }

    [Fact]
    public async Task Returns_Updated_and_upserts_profile_when_signal_computed()
    {
        var dims = 3;
        var embedding = MakeEmbedding(UnitVec(dims, nonZeroIndex: 1));
        var interactions = new[] { MakeInteraction(NewsInteractionType.Save) };

        var handler = MakeHandlerWithCapturingProfile(
            out var repo,
            profile: MakeProfile(dims),
            interactions: interactions,
            embeddings: [embedding],
            dims: dims);

        var result = await handler.HandleAsync(ValidCommand);

        Assert.Equal(UpdateNewsProfileResult.Updated, result);
        Assert.NotNull(repo.Upserted);
    }

    [Fact]
    public async Task Upserted_embedding_is_unit_length()
    {
        var dims = 4;
        var embedding = MakeEmbedding(UnitVec(dims, nonZeroIndex: 2));
        var interactions = new[] { MakeInteraction(NewsInteractionType.Save) };

        var handler = MakeHandlerWithCapturingProfile(
            out var repo,
            profile: MakeProfile(dims),
            interactions: interactions,
            embeddings: [embedding],
            dims: dims);

        await handler.HandleAsync(ValidCommand);

        var vec = repo.Upserted!.LongTermEmbedding.ToArray();
        var magnitude = Math.Sqrt(vec.Sum(x => (double)x * x));
        Assert.True(Math.Abs(magnitude - 1.0) < 1e-5, $"Expected unit vector, got magnitude {magnitude}");
    }

    [Fact]
    public async Task Profile_blends_toward_signal_at_alpha_085()
    {
        // Profile starts at [1, 0, 0]. Signal comes from a save on an article at [0, 1, 0].
        // After blend: result ≈ normalize(0.85*[1,0,0] + 0.15*[0,1,0]) → [1,0,0] component dominant.
        var dims = 3;
        var profile = new NewsUserProfile
        {
            UserId = 1,
            LongTermEmbedding = new Vector(new float[] { 1f, 0f, 0f }),
            SeedText = "seed",
            SeededAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        // Article embedding in the [0,1,0] direction.
        var embedding = MakeEmbedding(new float[] { 0f, 1f, 0f });
        var interactions = new[] { MakeInteraction(NewsInteractionType.Save) };

        var handler = MakeHandlerWithCapturingProfile(
            out var repo,
            profile: profile,
            interactions: interactions,
            embeddings: [embedding],
            dims: dims);

        await handler.HandleAsync(ValidCommand);

        var result = repo.Upserted!.LongTermEmbedding.ToArray();
        // dim[0] should be larger than dim[1] because alpha=0.85 preserves the old direction.
        Assert.True(result[0] > result[1],
            $"Expected dim[0] ({result[0]}) > dim[1] ({result[1]}) after 85/15 blend");
    }

    [Fact]
    public async Task Read_with_long_dwell_has_higher_weight_than_short_dwell()
    {
        // Two separate profiles — one with a long dwell read, one with a short dwell read.
        // The long dwell profile should move more toward the article direction.
        var dims = 3;
        var articleVec = new float[] { 0f, 1f, 0f };

        async Task<float[]> RunWithDwell(int dwell)
        {
            var profile = new NewsUserProfile
            {
                UserId = 1,
                LongTermEmbedding = new Vector(new float[] { 1f, 0f, 0f }),
                SeedText = "seed",
                SeededAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            };
            var embedding = MakeEmbedding(articleVec);
            var interactions = new[] { MakeInteraction(NewsInteractionType.Read, dwell) };

            var handler = MakeHandlerWithCapturingProfile(
                out var repo,
                profile: profile,
                interactions: interactions,
                embeddings: [embedding],
                avgDwell: 60.0,
                dims: dims);

            await handler.HandleAsync(ValidCommand);
            return repo.Upserted!.LongTermEmbedding.ToArray();
        }

        var shortResult = await RunWithDwell(5);    // well below average
        var longResult  = await RunWithDwell(180);  // well above average (3× avg)

        // Long dwell should push more toward dim[1] (the article direction).
        Assert.True(longResult[1] > shortResult[1],
            $"Expected long dwell dim[1] ({longResult[1]}) > short dwell dim[1] ({shortResult[1]})");
    }

    // ---------------------------------------------------------------------------
    // Validation tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Throws_ValidationException_for_zero_user_id()
    {
        var handler = MakeHandler(profile: MakeProfile());
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new UpdateNewsProfileCommand(0, 7)));
    }

    [Fact]
    public async Task Throws_ValidationException_for_window_days_out_of_range()
    {
        var handler = MakeHandler(profile: MakeProfile());
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new UpdateNewsProfileCommand(1, 0)));
    }
}
