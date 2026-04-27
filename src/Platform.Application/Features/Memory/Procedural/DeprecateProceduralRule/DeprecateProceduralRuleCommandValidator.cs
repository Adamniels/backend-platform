using FluentValidation;

namespace Platform.Application.Features.Memory.Procedural.DeprecateProceduralRule;

public sealed class DeprecateProceduralRuleCommandValidator : AbstractValidator<DeprecateProceduralRuleCommand>
{
    public DeprecateProceduralRuleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
