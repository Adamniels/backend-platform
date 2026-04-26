using FluentValidation;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.CreateProceduralRule;

public sealed class CreateProceduralRuleCommandValidator : AbstractValidator<CreateProceduralRuleCommand>
{
    public CreateProceduralRuleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .Must(id => id == 0 || id == MemoryUser.DefaultId)
            .WithMessage("UserId must be omitted, 0, or 1 in the current deployment.");
        RuleFor(x => x.WorkflowType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.RuleName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.RuleContent).NotNull();
        RuleFor(x => x.Source).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AuthorityWeight).InclusiveBetween(0, 1);
    }
}
