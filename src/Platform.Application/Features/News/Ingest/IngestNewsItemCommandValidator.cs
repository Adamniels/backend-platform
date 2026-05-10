using FluentValidation;

namespace Platform.Application.Features.News.Ingest;

public sealed class IngestNewsItemCommandValidator : AbstractValidator<IngestNewsItemCommand>
{
    public IngestNewsItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.Url).NotEmpty().MaximumLength(4096);
        RuleFor(x => x.Source).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Body).NotNull().MaximumLength(1_000_000);
        RuleFor(x => x.Author).MaximumLength(512).When(x => x.Author is not null);
        RuleFor(x => x.SourceFeedUrl).MaximumLength(2048).When(x => x.SourceFeedUrl is not null);
    }
}
