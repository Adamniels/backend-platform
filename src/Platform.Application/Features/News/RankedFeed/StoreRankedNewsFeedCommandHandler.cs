using System.Text.Json;
using FluentValidation;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;

namespace Platform.Application.Features.News.RankedFeed;

public sealed class StoreRankedNewsFeedCommandHandler(
    IValidator<StoreRankedNewsFeedCommand> validator,
    INewsRankedFeedRepository rankedFeedRepo)
{
    public async Task<StoreRankedNewsFeedResult> HandleAsync(
        StoreRankedNewsFeedCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var entriesJson = JsonSerializer.Serialize(command.Rankings);

        var feed = new NewsRankedFeed
        {
            UserId     = command.UserId,
            EntriesJson = entriesJson,
            ModelUsed  = command.ModelUsed,
            RankedAt   = DateTimeOffset.UtcNow,
        };

        await rankedFeedRepo.UpsertAsync(feed, cancellationToken).ConfigureAwait(false);
        return StoreRankedNewsFeedResult.Stored;
    }
}
