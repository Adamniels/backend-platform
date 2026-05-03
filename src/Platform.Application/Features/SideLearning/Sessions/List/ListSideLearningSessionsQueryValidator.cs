using FluentValidation;

namespace Platform.Application.Features.SideLearning.Sessions.List;

public sealed class ListSideLearningSessionsQueryValidator : AbstractValidator<ListSideLearningSessionsQuery>
{
    public ListSideLearningSessionsQueryValidator()
    {
        RuleFor(x => x.Lifecycle)
            .NotEmpty()
            .Must(v => string.Equals(v, "ongoing", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(v, "archive", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Query parameter 'lifecycle' must be 'ongoing' or 'archive'.");
        RuleFor(x => x.Take).InclusiveBetween(1, 50);
    }
}
