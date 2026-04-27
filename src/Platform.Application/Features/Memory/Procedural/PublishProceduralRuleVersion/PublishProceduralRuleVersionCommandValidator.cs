using FluentValidation;

namespace Platform.Application.Features.Memory.Procedural.PublishProceduralRuleVersion;

public sealed class PublishProceduralRuleVersionCommandValidator : AbstractValidator<PublishProceduralRuleVersionCommand>
{
    public PublishProceduralRuleVersionCommandValidator()
    {
        RuleFor(x => x.BasisRuleId).GreaterThan(0);
        RuleFor(x => x.RuleContent).NotEmpty();
        RuleFor(x => x.AuthorityWeight)
            .InclusiveBetween(0, 1)
            .When(x => x.AuthorityWeight is not null);
    }
}
