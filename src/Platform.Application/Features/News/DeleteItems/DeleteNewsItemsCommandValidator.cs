using FluentValidation;

namespace Platform.Application.Features.News.DeleteItems;

public sealed class DeleteNewsItemsCommandValidator : AbstractValidator<DeleteNewsItemsCommand>
{
    public const int MaxBatch = 100;

    public DeleteNewsItemsCommandValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("At least one id is required.");
        RuleFor(x => x.Ids)
            .Must(ids => ids.Count <= MaxBatch)
            .WithMessage($"At most {MaxBatch} ids may be deleted at once.");
        RuleForEach(x => x.Ids)
            .NotEmpty()
            .MaximumLength(64)
            .Matches(@"^[\w\-]+$")
            .WithMessage("Each id must be a non-empty alphanumeric string (with hyphens allowed).");
    }
}
