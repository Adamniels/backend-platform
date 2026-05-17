using FluentValidation;
using Platform.Application.Abstractions.News;
using Platform.Domain.Features.News;

namespace Platform.Application.Features.News.RecordInteraction;

public sealed class RecordNewsInteractionCommandHandler(
    IValidator<RecordNewsInteractionCommand> validator,
    INewsInteractionRepository interactions)
{
    public async Task HandleAsync(
        RecordNewsInteractionCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var type = Enum.Parse<NewsInteractionType>(command.Type, ignoreCase: true);

        var interaction = new NewsInteraction
        {
            UserId = command.UserId,
            NewsItemId = command.NewsItemId,
            Type = type,
            DwellSeconds = command.DwellSeconds,
            RecordedAt = DateTimeOffset.UtcNow,
        };

        await interactions.InsertAsync(interaction, cancellationToken).ConfigureAwait(false);
    }
}
