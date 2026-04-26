using FluentValidation;
using Platform.Application.Features.Memory.Review;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue.CreateItem;

public sealed class CreateReviewQueueItemCommandValidator : AbstractValidator<CreateReviewQueueItemCommand>
{
    public CreateReviewQueueItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .Must(id => id == 0 || id == MemoryUser.DefaultId)
            .WithMessage("UserId must be omitted, 0, or 1 in the current deployment.");
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
                    if (!Enum.TryParse<MemoryReviewProposalType>(
                            cmd.ProposalType,
                            ignoreCase: true,
                            out var pt) ||
                        pt != MemoryReviewProposalType.NewSemantic)
                    {
                        return true;
                    }

                    try
                    {
                        _ = MemoryReviewProposalJson.ParseNewSemantic(cmd.ProposedChangeJson);
                        return true;
                    }
                    catch (MemoryDomainException)
                    {
                        return false;
                    }
                })
            .When(
                x => Enum.TryParse<MemoryReviewProposalType>(
                    x.ProposalType,
                    ignoreCase: true,
                    out var t) &&
                    t == MemoryReviewProposalType.NewSemantic)
            .WithMessage("ProposedChangeJson must be a valid NewSemantic proposal for this type.");
    }
}
