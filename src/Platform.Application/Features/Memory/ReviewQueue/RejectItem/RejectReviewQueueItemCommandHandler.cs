using Platform.Application.Abstractions.Memory.Review;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue.RejectItem;

public sealed class RejectReviewQueueItemCommandHandler(IMemoryReviewService reviews)
{
    public async Task HandleAsync(
        RejectReviewQueueItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        await reviews
            .RejectAsync(command.ReviewItemId, userId, command.Reason, cancellationToken)
            .ConfigureAwait(false);
    }
}
