using FluentValidation;

namespace Platform.Application.Features.Memory.Procedural.ActivateProceduralRule;

public sealed class ActivateProceduralRuleCommandValidator : AbstractValidator<ActivateProceduralRuleCommand>
{
    public ActivateProceduralRuleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
