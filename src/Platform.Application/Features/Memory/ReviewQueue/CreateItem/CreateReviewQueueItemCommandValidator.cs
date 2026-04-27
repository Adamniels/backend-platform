using FluentValidation;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;

namespace Platform.Application.Features.Memory.ReviewQueue.CreateItem;

public sealed class CreateReviewQueueItemCommandValidator : AbstractValidator<CreateReviewQueueItemCommand>
{
    public CreateReviewQueueItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(512);
        RuleFor(x => x.Summary)
            .MaximumLength(4000);
        RuleFor(x => x.Priority)
            .InclusiveBetween(0, 1_000_000);
        RuleFor(x => x.ProposalType)
            .Must(
                s => Enum.TryParse<MemoryReviewProposalType>(s, ignoreCase: true, out var t) &&
                    t is not MemoryReviewProposalType.Unspecified)
            .WithMessage("ProposalType must be a known proposal type.");
        RuleFor(x => x)
            .Must(
                cmd =>
                {
                    if (!Enum.TryParse<MemoryReviewProposalType>(cmd.ProposalType, ignoreCase: true, out var pt))
                    {
                        return false;
                    }

                    try
                    {
                        MemoryReviewProposalJson.ValidateForProposalType(pt, cmd.ProposedChangeJson);
                        return true;
                    }
                    catch (MemoryDomainException)
                    {
                        return false;
                    }
                })
            .WithMessage("ProposedChangeJson must be a valid payload for the selected ProposalType.");
    }
}
