using FluentValidation;

namespace Platform.Application.Features.News.Profile;

public sealed class UpdateNewsProfileCommandValidator : AbstractValidator<UpdateNewsProfileCommand>
{
    public UpdateNewsProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be a positive integer.");

        RuleFor(x => x.WindowDays)
            .InclusiveBetween(1, 90)
            .WithMessage("WindowDays must be between 1 and 90.");
    }
}
