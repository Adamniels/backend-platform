using FluentValidation;
using Platform.Application.Abstractions.News;
using Platform.Contracts.V1.News;

namespace Platform.Application.Features.News.DeleteItems;

public sealed class DeleteNewsItemsCommandHandler(
    IValidator<DeleteNewsItemsCommand> validator,
    INewsDeleteRepository deleteRepository)
{
    public async Task<DeleteNewsItemsV1Response> HandleAsync(
        DeleteNewsItemsCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        var unique = command.Ids.Distinct(StringComparer.Ordinal).ToList();
        var deleted = await deleteRepository
            .DeleteByIdsAsync(unique, cancellationToken)
            .ConfigureAwait(false);

        return new DeleteNewsItemsV1Response(deleted);
    }
}
