using FluentValidation;

namespace Platform.Application.Features.Memory.Semantic.CreateSemanticMemory;

public sealed class CreateSemanticMemoryCommandValidator : AbstractValidator<CreateSemanticMemoryCommand>
{
    public CreateSemanticMemoryCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty();
        RuleFor(x => x.Claim)
            .NotEmpty();
        RuleFor(x => x.EventId)
            .GreaterThan(0);
        RuleFor(x => x.Confidence)
            .InclusiveBetween(0, 1);
        RuleFor(x => x.EvidenceStrength)
            .InclusiveBetween(0, 1);
        When(
            x => x.EvidenceReliabilityWeight is not null,
            () => RuleFor(x => x.EvidenceReliabilityWeight!.Value).InclusiveBetween(0, 1));
        RuleFor(x => x.EvidenceReason)
            .MaximumLength(2048)
            .When(x => x.EvidenceReason is not null);
        RuleFor(x => x.EvidenceSourceId)
            .MaximumLength(512)
            .When(x => x.EvidenceSourceId is not null);
        RuleFor(x => x.EvidenceSchemaVersion)
            .MaximumLength(64)
            .When(x => x.EvidenceSchemaVersion is not null);
        RuleFor(x => x.Domain)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.Domain));
        RuleFor(x => x.Key)
            .MaximumLength(256);
        When(
            x => x.AuthorityWeight is not null,
            () => RuleFor(x => x.AuthorityWeight!.Value).InclusiveBetween(0, 1));
    }
}
