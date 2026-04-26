using FluentValidation;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.DeprecateProceduralRule;

public sealed class DeprecateProceduralRuleCommandValidator : AbstractValidator<DeprecateProceduralRuleCommand>
{
    public DeprecateProceduralRuleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .Must(id => id == 0 || id == MemoryUser.DefaultId)
            .WithMessage("UserId must be omitted, 0, or 1 in the current deployment.");
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
