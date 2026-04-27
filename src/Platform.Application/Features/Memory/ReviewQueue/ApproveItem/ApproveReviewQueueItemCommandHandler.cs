using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.ReviewQueue.ApproveItem;

public sealed class ApproveReviewQueueItemCommandHandler(
    IMemoryReviewService reviews,
    IMemoryUserContextResolver userResolver)
{
    public async Task<ApproveMemoryReviewQueueItemV1Response> HandleAsync(
        ApproveReviewQueueItemCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(command.UserId);
        var r = await reviews
            .ApproveAsync(command.ReviewItemId, userId, command.ReviewNotes, cancellationToken)
            .ConfigureAwait(false);
        return new ApproveMemoryReviewQueueItemV1Response
        {
            SemanticMemoryId = r.SemanticMemoryId,
            ProceduralRuleId = r.ProceduralRuleId,
        };
    }
}
