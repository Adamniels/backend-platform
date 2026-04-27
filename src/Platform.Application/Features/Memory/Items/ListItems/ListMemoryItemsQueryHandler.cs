using Platform.Application.Abstractions.Memory.Items;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Items.ListItems;

public sealed class ListMemoryItemsQueryHandler(
    IMemoryItemReadRepository items,
    IMemoryUserContextResolver userResolver)
{
    public async Task<IReadOnlyList<MemoryItemSummaryV1Dto>> HandleAsync(
        ListMemoryItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = userResolver.Resolve(query.UserId);
        return await items.ListSummariesForUserAsync(id, cancellationToken).ConfigureAwait(false);
    }
}
