using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Users;

namespace Platform.Application.Features.Memory.ReviewQueue.RejectItem;

public sealed class RejectReviewQueueItemCommandHandler(
    IMemoryReviewService reviews,
    IMemoryUserContextResolver userResolver)
{
    public async Task HandleAsync(
        RejectReviewQueueItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(command.UserId);
        await reviews
            .RejectAsync(command.ReviewItemId, userId, command.Reason, cancellationToken)
            .ConfigureAwait(false);
    }
}
