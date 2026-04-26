using FluentValidation;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.ReviewQueue.PatchItem;

public sealed class PatchReviewQueueItemCommandValidator : AbstractValidator<PatchReviewQueueItemCommand>
{
    public PatchReviewQueueItemCommandValidator()
    {
        RuleFor(x => x.UserId)
            .Must(id => id == 0 || id == MemoryUser.DefaultId)
            .WithMessage("UserId must be omitted, 0, or 1 in the current deployment.");
        RuleFor(x => x)
            .Must(
                c => !string.IsNullOrWhiteSpace(c.Title) || c.Summary is not null ||
                    c.ProposedChangeJson is not null)
            .WithMessage("At least one of title, summary, or proposedChangeJson must be provided.");
        RuleFor(x => x.Title)
            .MaximumLength(512)
            .When(x => !string.IsNullOrWhiteSpace(x.Title));
        RuleFor(x => x.Summary)
            .MaximumLength(4000)
            .When(x => x.Summary is not null);
    }
}
