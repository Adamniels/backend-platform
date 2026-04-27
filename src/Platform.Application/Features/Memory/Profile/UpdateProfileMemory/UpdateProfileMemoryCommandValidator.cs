using FluentValidation;
using Platform.Domain.Features.Memory;

namespace Platform.Application.Features.Memory.Profile.UpdateProfileMemory;

public sealed class UpdateProfileMemoryCommandValidator : AbstractValidator<UpdateProfileMemoryCommand>
{
    public UpdateProfileMemoryCommandValidator()
    {
        RuleFor(x => x.CoreInterests)
            .NotNull()
            .Must(x => x!.Count <= ExplicitUserProfileContent.MaxListSize)
            .WithMessage(_ => $"At most {ExplicitUserProfileContent.MaxListSize} core interests are allowed.");
        RuleForEach(x => x.CoreInterests)
            .NotEmpty()
            .MaximumLength(ExplicitUserProfileContent.MaxStringLength);

        RuleFor(x => x.SecondaryInterests)
            .NotNull()
            .Must(x => x!.Count <= ExplicitUserProfileContent.MaxListSize)
            .WithMessage(_ => $"At most {ExplicitUserProfileContent.MaxListSize} secondary interests are allowed.");
        RuleForEach(x => x.SecondaryInterests)
            .NotEmpty()
            .MaximumLength(ExplicitUserProfileContent.MaxStringLength);

        RuleFor(x => x.Goals)
            .NotNull()
            .Must(x => x!.Count <= ExplicitUserProfileContent.MaxListSize)
            .WithMessage(_ => $"At most {ExplicitUserProfileContent.MaxListSize} goals are allowed.");
        RuleForEach(x => x.Goals)
            .NotEmpty()
            .MaximumLength(ExplicitUserProfileContent.MaxStringLength);

        RuleFor(x => x.Preferences)
            .NotNull()
            .Must(x => x!.Count <= ExplicitUserProfileContent.MaxListSize)
            .WithMessage(_ => $"At most {ExplicitUserProfileContent.MaxListSize} preferences are allowed.");
        RuleForEach(x => x.Preferences)
            .ChildRules(
                p =>
                {
                    p.RuleFor(v => v!.Key)
                        .NotEmpty()
                        .MaximumLength(ExplicitUserProfileContent.MaxKeyLength);
                    p.RuleFor(v => v!.Value)
                        .MaximumLength(ExplicitUserProfileContent.MaxValueLength);
                });

        RuleFor(x => x.ActiveProjects)
            .NotNull()
            .Must(x => x!.Count <= ExplicitUserProfileContent.MaxListSize)
            .WithMessage(_ => $"At most {ExplicitUserProfileContent.MaxListSize} active projects are allowed.");
        RuleForEach(x => x.ActiveProjects)
            .ChildRules(
                p =>
                {
                    p.RuleFor(v => v!.Name)
                        .NotEmpty()
                        .MaximumLength(ExplicitUserProfileContent.MaxNameLength);
                    p.RuleFor(v => v!.ExternalId)
                        .MaximumLength(ExplicitUserProfileContent.MaxProjectExternalIdLength);
                });

        RuleFor(x => x.SkillLevels)
            .NotNull()
            .Must(x => x!.Count <= ExplicitUserProfileContent.MaxListSize)
            .WithMessage(_ => $"At most {ExplicitUserProfileContent.MaxListSize} skill levels are allowed.");
        RuleForEach(x => x.SkillLevels)
            .ChildRules(
                s =>
                {
                    s.RuleFor(v => v!.Name)
                        .NotEmpty()
                        .MaximumLength(ExplicitUserProfileContent.MaxNameLength);
                    s.RuleFor(v => v!.Level)
                        .InclusiveBetween(0d, 1d);
                });
    }
}
