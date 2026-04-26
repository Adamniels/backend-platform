using FluentValidation;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.UpdateProceduralRulePriority;

public sealed class UpdateProceduralRulePriorityCommandValidator : AbstractValidator<UpdateProceduralRulePriorityCommand>
{
    public UpdateProceduralRulePriorityCommandValidator()
    {
        RuleFor(x => x.UserId)
            .Must(id => id == 0 || id == MemoryUser.DefaultId)
            .WithMessage("UserId must be omitted, 0, or 1 in the current deployment.");
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
    }
}
