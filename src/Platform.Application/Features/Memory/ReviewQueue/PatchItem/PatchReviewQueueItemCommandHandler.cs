using FluentValidation;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Application.Features.Memory.ReviewQueue;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;

namespace Platform.Application.Features.Memory.ReviewQueue.PatchItem;

public sealed class PatchReviewQueueItemCommandHandler(
    IValidator<PatchReviewQueueItemCommand> validator,
    IMemoryReviewService reviews,
    IMemoryUserContextResolver userResolver)
{
    public async Task<MemoryReviewQueueItemV1Dto> HandleAsync(
        PatchReviewQueueItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = userResolver.Resolve(command.UserId);
        await reviews
            .UpdatePendingAsync(
                command.ReviewItemId,
                userId,
                command.Title,
                command.Summary,
                command.ProposedChangeJson,
                cancellationToken)
            .ConfigureAwait(false);
        var row = await reviews
            .GetByIdForUserAsync(command.ReviewItemId, userId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
        {
            throw new MemoryDomainException("Review item not found after update.");
        }

        return MemoryReviewQueueItemMapper.ToV1Dto(row);
    }
}
