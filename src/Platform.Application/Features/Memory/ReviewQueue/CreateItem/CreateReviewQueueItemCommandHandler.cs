using FluentValidation;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Features.Memory.ReviewQueue;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue.CreateItem;

public sealed class CreateReviewQueueItemCommandHandler(
    IValidator<CreateReviewQueueItemCommand> validator,
    IMemoryReviewService reviews)
{
    public async Task<MemoryReviewQueueItemV1Dto> HandleAsync(
        CreateReviewQueueItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        if (!Enum.TryParse<MemoryReviewProposalType>(
                command.ProposalType,
                ignoreCase: true,
                out var proposalType))
        {
            throw new MemoryDomainException("Invalid ProposalType.");
        }

        var at = DateTimeOffset.UtcNow;
        var item = MemoryReviewQueueItem.Propose(
            userId,
            proposalType,
            command.Title,
            command.Summary,
            command.ProposedChangeJson,
            command.EvidenceJson,
            command.Priority,
            at);
        var saved = await reviews
            .CreatePendingAsync(item, cancellationToken)
            .ConfigureAwait(false);
        return MemoryReviewQueueItemMapper.ToV1Dto(saved);
    }
}
