using FluentValidation;

namespace Platform.Application.Features.Memory.Semantic.AttachSemanticMemoryEvidence;

public sealed class AttachSemanticMemoryEvidenceCommandValidator
    : AbstractValidator<AttachSemanticMemoryEvidenceCommand>
{
    public AttachSemanticMemoryEvidenceCommandValidator()
    {
        RuleFor(x => x.EventId)
            .GreaterThan(0);
        RuleFor(x => x.Strength)
            .InclusiveBetween(0, 1);
        RuleFor(x => x.ReinforceConfidenceDelta)
            .InclusiveBetween(-1, 1);
        RuleFor(x => x.Reason)
            .MaximumLength(2048)
            .When(x => x.Reason is not null);
    }
}
