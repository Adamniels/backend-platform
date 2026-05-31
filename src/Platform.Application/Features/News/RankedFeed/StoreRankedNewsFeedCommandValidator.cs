using FluentValidation;

namespace Platform.Application.Features.News.RankedFeed;

public sealed class StoreRankedNewsFeedCommandValidator : AbstractValidator<StoreRankedNewsFeedCommand>
{
    public StoreRankedNewsFeedCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be a positive integer.");

        RuleFor(x => x.ModelUsed)
            .NotEmpty()
            .WithMessage("ModelUsed must not be empty.");

        RuleFor(x => x.Rankings)
            .NotNull()
            .Must(r => r.Count > 0)
            .WithMessage("Rankings must contain at least one entry.");

        RuleForEach(x => x.Rankings).ChildRules(entry =>
        {
            entry.RuleFor(e => e.NewsItemId).NotEmpty();
            entry.RuleFor(e => e.Score).InclusiveBetween(0, 100);
            entry.RuleFor(e => e.Explanation).NotEmpty();
        });
    }
}
