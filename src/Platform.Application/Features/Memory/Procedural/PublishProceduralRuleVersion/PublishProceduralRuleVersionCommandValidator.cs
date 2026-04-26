using FluentValidation;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.PublishProceduralRuleVersion;

public sealed class PublishProceduralRuleVersionCommandValidator : AbstractValidator<PublishProceduralRuleVersionCommand>
{
    public PublishProceduralRuleVersionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .Must(id => id == 0 || id == MemoryUser.DefaultId)
            .WithMessage("UserId must be omitted, 0, or 1 in the current deployment.");
        RuleFor(x => x.BasisRuleId).GreaterThan(0);
        RuleFor(x => x.RuleContent).NotEmpty();
        RuleFor(x => x.AuthorityWeight)
            .InclusiveBetween(0, 1)
            .When(x => x.AuthorityWeight is not null);
    }
}
