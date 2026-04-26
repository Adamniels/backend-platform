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
        RuleFor(x => x.EvidenceReason)
            .MaximumLength(2048)
            .When(x => x.EvidenceReason is not null);
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
