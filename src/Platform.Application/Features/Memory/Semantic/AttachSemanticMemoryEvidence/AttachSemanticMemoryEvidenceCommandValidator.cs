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
        When(
            x => x.ReliabilityWeight is not null,
            () => RuleFor(x => x.ReliabilityWeight!.Value).InclusiveBetween(0, 1));
        RuleFor(x => x.SourceId)
            .MaximumLength(512)
            .When(x => x.SourceId is not null);
        RuleFor(x => x.SchemaVersion)
            .MaximumLength(64)
            .When(x => x.SchemaVersion is not null);
    }
}
