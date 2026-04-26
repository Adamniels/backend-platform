using Platform.Application.Abstractions.SavedItems;
using Platform.Contracts.V1;

namespace Platform.Application.Features.SavedItems.ListSavedItems;

public sealed class ListSavedItemsQueryHandler(ISavedItemsReadRepository items)
{
    public async Task<IReadOnlyList<SavedItemSummaryDto>> HandleAsync(
        ListSavedItemsQuery _,
        CancellationToken cancellationToken = default) =>
        await items.ListAsync(cancellationToken).ConfigureAwait(false);
}
