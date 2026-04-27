using FluentValidation;

namespace Platform.Application.Features.Memory.Procedural.CreateProceduralRule;

public sealed class CreateProceduralRuleCommandValidator : AbstractValidator<CreateProceduralRuleCommand>
{
    public CreateProceduralRuleCommandValidator()
    {
        RuleFor(x => x.WorkflowType).NotEmpty().MaximumLength(128);
        RuleFor(x => x.RuleName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.RuleContent).NotNull();
        RuleFor(x => x.Source).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AuthorityWeight).InclusiveBetween(0, 1);
    }
}
