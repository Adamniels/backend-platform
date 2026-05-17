using FluentValidation;

namespace Platform.Application.Features.News.RecordInteraction;

public sealed class RecordNewsInteractionCommandValidator : AbstractValidator<RecordNewsInteractionCommand>
{
    private static readonly System.Text.RegularExpressions.Regex NewsItemIdPattern =
        new(@"^ni-[0-9a-f]{32}$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly HashSet<string> ValidTypes =
        new(StringComparer.OrdinalIgnoreCase) { "read", "save", "dismiss" };

    public RecordNewsInteractionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be a positive integer.");

        RuleFor(x => x.NewsItemId)
            .NotEmpty()
            .Matches(NewsItemIdPattern)
            .WithMessage("NewsItemId must match the pattern 'ni-<32 hex chars>'.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => ValidTypes.Contains(t))
            .WithMessage("Type must be one of: read, save, dismiss.");

        // DwellSeconds is required for 'read' and must be positive.
        RuleFor(x => x.DwellSeconds)
            .NotNull()
            .GreaterThan(0)
            .When(x => string.Equals(x.Type, "read", StringComparison.OrdinalIgnoreCase))
            .WithMessage("DwellSeconds is required and must be positive when Type is 'read'.");

        // DwellSeconds must be absent for non-read types.
        RuleFor(x => x.DwellSeconds)
            .Null()
            .When(x => !string.Equals(x.Type, "read", StringComparison.OrdinalIgnoreCase))
            .WithMessage("DwellSeconds must not be provided for 'save' or 'dismiss' interactions.");
    }
}
