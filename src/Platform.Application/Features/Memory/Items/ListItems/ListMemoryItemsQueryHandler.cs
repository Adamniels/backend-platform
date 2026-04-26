using Platform.Application.Abstractions.Memory.Items;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Items.ListItems;

public sealed class ListMemoryItemsQueryHandler(IMemoryItemReadRepository items)
{
    public async Task<IReadOnlyList<MemoryItemSummaryV1Dto>> HandleAsync(
        ListMemoryItemsQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = query.PrincipalId is 0 ? MemoryPrincipal.SingleTenantKey : query.PrincipalId;
        return await items.ListSummariesForPrincipalAsync(id, cancellationToken).ConfigureAwait(false);
    }
}
