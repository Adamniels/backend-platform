using FluentValidation;

namespace Platform.Application.Features.News.Profile;

public sealed class SeedNewsProfileCommandValidator : AbstractValidator<SeedNewsProfileCommand>
{
    public SeedNewsProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be a positive integer.");
    }
}
