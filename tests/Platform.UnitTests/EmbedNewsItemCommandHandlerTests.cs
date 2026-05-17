using FluentValidation;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Application.Features.News.Embed;
using Platform.Domain.Features.News;

namespace Platform.UnitTests;

public sealed class EmbedNewsItemCommandHandlerTests
{
    // ---------------------------------------------------------------------------
    // Test doubles
    // ---------------------------------------------------------------------------

    private sealed class StubEmbeddingRepository(bool exists) : INewsEmbeddingRepository
    {
        public NewsItemEmbedding? Upserted { get; private set; }

        public Task<bool> ExistsAsync(string newsItemId, string modelKey, CancellationToken cancellationToken = default) =>
            Task.FromResult(exists);

        public Task UpsertAsync(NewsItemEmbedding embedding, CancellationToken cancellationToken = default)
        {
            Upserted = embedding;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<NewsItemEmbedding>> GetByNewsItemIdsAsync(
            IEnumerable<string> newsItemIds,
            string modelKey,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<NewsItemEmbedding>>([]);
    }

    private sealed class StubNewsReadRepository(string? body) : INewsReadRepository
    {
        public Task<IReadOnlyList<Platform.Contracts.V1.NewsItemSummaryDto>> ListFeedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Platform.Contracts.V1.NewsItemSummaryDto>>([]);

        public Task<string?> GetBodyByIdAsync(string id, CancellationToken cancellationToken = default) =>
            Task.FromResult(body);

        public Task<IReadOnlyList<Platform.Contracts.V1.NewsItemSummaryDto>> GetByIdsAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Platform.Contracts.V1.NewsItemSummaryDto>>([]);
    }

    private sealed class StubEmbeddingGenerator(float[]? result) : IMemoryEmbeddingGenerator
    {
        public string ModelKey => "stub-v1";
        public int Dimensions => result?.Length ?? 1536;

        public Task<float[]?> TryEmbedRecallQueryAsync(string? text, CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }

    private static EmbedNewsItemCommandHandler MakeHandler(
        bool embeddingExists = false,
        string? bodyText = "Body text",
        float[]? embeddingResult = null) =>
        new(
            new EmbedNewsItemCommandValidator(),
            new StubNewsReadRepository(bodyText),
            new StubEmbeddingRepository(embeddingExists),
            new StubEmbeddingGenerator(embeddingResult ?? [0.1f, 0.2f, 0.3f]));

    private static readonly EmbedNewsItemCommand ValidCommand = new("ni-" + new string('a', 32));

    // ---------------------------------------------------------------------------
    // D2 tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Returns_Embedded_when_body_exists_and_generator_succeeds()
    {
        var handler = MakeHandler(embeddingExists: false, bodyText: "Some article body");
        var result = await handler.HandleAsync(ValidCommand);
        Assert.Equal(EmbedNewsItemResult.Embedded, result);
    }

    [Fact]
    public async Task Returns_Skipped_when_embedding_already_exists()
    {
        var handler = MakeHandler(embeddingExists: true, bodyText: "Some article body");
        var result = await handler.HandleAsync(ValidCommand);
        Assert.Equal(EmbedNewsItemResult.Skipped, result);
    }

    [Fact]
    public async Task Returns_Error_when_body_is_null()
    {
        var handler = MakeHandler(embeddingExists: false, bodyText: null);
        var result = await handler.HandleAsync(ValidCommand);
        Assert.Equal(EmbedNewsItemResult.Error, result);
    }

    [Fact]
    public async Task Returns_Error_when_generator_returns_null()
    {
        var handler = new EmbedNewsItemCommandHandler(
            new EmbedNewsItemCommandValidator(),
            new StubNewsReadRepository("Body"),
            new StubEmbeddingRepository(false),
            new StubEmbeddingGenerator(null));
        var result = await handler.HandleAsync(ValidCommand);
        Assert.Equal(EmbedNewsItemResult.Error, result);
    }

    [Fact]
    public async Task Throws_ValidationException_for_invalid_id()
    {
        var handler = MakeHandler();
        await Assert.ThrowsAsync<ValidationException>(
            () => handler.HandleAsync(new EmbedNewsItemCommand("bad-id")));
    }
}
