using FluentValidation;
using Pgvector;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;

namespace Platform.Application.Features.News.Embed;

public sealed class EmbedNewsItemCommandHandler(
    IValidator<EmbedNewsItemCommand> validator,
    INewsReadRepository newsRead,
    INewsEmbeddingRepository embedRepo,
    IMemoryEmbeddingGenerator embeddingGenerator)
{
    // text-embedding-3-small has an 8191 token limit (~6 000 words).
    // Embedding title + first portion of body gives the best semantic signal.
    private const int MaxBodyChars = 5_500;

    public async Task<EmbedNewsItemResult> HandleAsync(
        EmbedNewsItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        // Skip if already embedded for this model.
        if (await embedRepo
                .ExistsAsync(command.NewsItemId, embeddingGenerator.ModelKey, cancellationToken)
                .ConfigureAwait(false))
        {
            return EmbedNewsItemResult.Skipped;
        }

        var body = await newsRead
            .GetBodyByIdAsync(command.NewsItemId, cancellationToken)
            .ConfigureAwait(false);

        if (body is null)
            return EmbedNewsItemResult.Error;

        var truncated = body.Length > MaxBodyChars ? body[..MaxBodyChars] : body;
        var vector = await embeddingGenerator
            .TryEmbedRecallQueryAsync(truncated, cancellationToken)
            .ConfigureAwait(false);

        if (vector is null)
            return EmbedNewsItemResult.Error;

        var embedding = new NewsItemEmbedding
        {
            NewsItemId = command.NewsItemId,
            EmbeddingModelKey = embeddingGenerator.ModelKey,
            Dimensions = embeddingGenerator.Dimensions,
            Embedding = new Vector(vector),
            EmbeddedAt = DateTimeOffset.UtcNow,
        };

        await embedRepo.UpsertAsync(embedding, cancellationToken).ConfigureAwait(false);
        return EmbedNewsItemResult.Embedded;
    }
}

public enum EmbedNewsItemResult
{
    Embedded,
    Skipped,
    Error,
}
