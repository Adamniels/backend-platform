using Platform.Application.Abstractions.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue.ApproveItem;

public sealed class ApproveReviewQueueItemCommandHandler(IMemoryReviewService reviews)
{
    public async Task<ApproveMemoryReviewQueueItemV1Response> HandleAsync(
        ApproveReviewQueueItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        var id = await reviews
            .ApproveAsync(command.ReviewItemId, userId, command.ReviewNotes, cancellationToken)
            .ConfigureAwait(false);
        return new ApproveMemoryReviewQueueItemV1Response { SemanticMemoryId = id };
    }
}
