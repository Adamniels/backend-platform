using FluentValidation;

namespace Platform.Application.Features.Memory.Procedural.UpdateProceduralRulePriority;

public sealed class UpdateProceduralRulePriorityCommandValidator : AbstractValidator<UpdateProceduralRulePriorityCommand>
{
    public UpdateProceduralRulePriorityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
    }
}
