using FluentValidation;

namespace Platform.Application.Features.News.Embed;

public sealed class EmbedNewsItemCommandValidator : AbstractValidator<EmbedNewsItemCommand>
{
    // ni- prefix followed by 32 hex characters (Guid formatted with N specifier).
    private static readonly System.Text.RegularExpressions.Regex NewsItemIdPattern =
        new(@"^ni-[0-9a-f]{32}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public EmbedNewsItemCommandValidator()
    {
        RuleFor(x => x.NewsItemId)
            .NotEmpty()
            .Matches(NewsItemIdPattern)
            .WithMessage("NewsItemId must match the pattern 'ni-<32 hex chars>'.");
    }
}
