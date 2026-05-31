using FluentValidation;

namespace Platform.Application.Features.News.Profile;

public sealed class UpdateNewsActiveContextCommandValidator : AbstractValidator<UpdateNewsActiveContextCommand>
{
    public UpdateNewsActiveContextCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be a positive integer.");
    }
}
