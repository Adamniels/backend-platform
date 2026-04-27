using FluentValidation;

namespace Platform.Application.Features.Memory.ReviewQueue.PatchItem;

public sealed class PatchReviewQueueItemCommandValidator : AbstractValidator<PatchReviewQueueItemCommand>
{
    public PatchReviewQueueItemCommandValidator()
    {
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
