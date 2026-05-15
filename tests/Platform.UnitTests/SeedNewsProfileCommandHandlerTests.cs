using FluentValidation;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Application.Features.News.Profile;
using Platform.Domain.Features.News;

namespace Platform.UnitTests;

public sealed class SeedNewsProfileCommandHandlerTests
{
    // ---------------------------------------------------------------------------
    // Test doubles
    // ---------------------------------------------------------------------------

    private sealed class StubNewsProfileRepository(bool exists) : INewsProfileRepository
    {
        public NewsUserProfile? Upserted { get; private set; }

        public Task<bool> ExistsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);

        public Task UpsertAsync(NewsUserProfile profile, CancellationToken cancellationToken = default)
        {
            Upserted = profile;
            return Task.CompletedTask;
        }
    }

    private sealed class StubUserInterestProvider(UserInterestSnapshot snapshot) : IUserInterestProvider
    {
        public Task<UserInterestSnapshot> GetInterestsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(snapshot);
    }

    private sealed class StubEmbeddingGenerator(float[]? result) : IMemoryEmbeddingGenerator
    {
        public string ModelKey => "stub-v1";
        public int Dimensions => result?.Length ?? 1536;

        public Task<float[]?> TryEmbedRecallQueryAsync(string? text, CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }

    private static readonly UserInterestSnapshot RichSnapshot = new(
        ["software engineering", "AI"],
        ["productivity"],
        ["ship faster"],
        [new UserInterestProjectSnapshot("Platform", null)]);

    private static readonly UserInterestSnapshot EmptySnapshot = new([], [], [], []);

    private static SeedNewsProfileCommandHandler MakeHandler(
        bool profileExists = false,
        UserInterestSnapshot? snapshot = null,
        float[]? embeddingResult = null) =>
        new(
            new SeedNewsProfileCommandValidator(),
            new StubUserInterestProvider(snapshot ?? RichSnapshot),
            new StubNewsProfileRepository(profileExists),
            new StubEmbeddingGenerator(embeddingResult ?? [0.1f, 0.2f, 0.3f]));

    // ---------------------------------------------------------------------------
    // D2 tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Returns_Seeded_on_first_call()
    {
        var handler = MakeHandler(profileExists: false);
        var result = await handler.HandleAsync(new SeedNewsProfileCommand(1));
        Assert.Equal(SeedNewsProfileResult.Seeded, result);
    }

    [Fact]
    public async Task Returns_Exists_when_profile_already_present()
    {
        var handler = MakeHandler(profileExists: true);
        var result = await handler.HandleAsync(new SeedNewsProfileCommand(1));
        Assert.Equal(SeedNewsProfileResult.Exists, result);
    }

    [Fact]
    public async Task Returns_Error_when_generator_returns_null()
    {
        var handler = new SeedNewsProfileCommandHandler(
            new SeedNewsProfileCommandValidator(),
            new StubUserInterestProvider(RichSnapshot),
            new StubNewsProfileRepository(false),
            new StubEmbeddingGenerator(null));
        var result = await handler.HandleAsync(new SeedNewsProfileCommand(1));
        Assert.Equal(SeedNewsProfileResult.Error, result);
    }

    [Fact]
    public async Task Returns_Error_when_seed_text_is_empty_and_generator_returns_null()
    {
        // Empty snapshot produces empty seed text; embedding generator returns null for empty input.
        var handler = new SeedNewsProfileCommandHandler(
            new SeedNewsProfileCommandValidator(),
            new StubUserInterestProvider(EmptySnapshot),
            new StubNewsProfileRepository(false),
            new StubEmbeddingGenerator(null));
        var result = await handler.HandleAsync(new SeedNewsProfileCommand(1));
        Assert.Equal(SeedNewsProfileResult.Error, result);
    }

    [Fact]
    public async Task Throws_ValidationException_for_zero_user_id()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new SeedNewsProfileCommand(0)));
    }
}
