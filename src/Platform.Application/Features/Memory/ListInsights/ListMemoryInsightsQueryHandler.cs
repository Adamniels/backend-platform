using Platform.Application.Abstractions.Memory;
using Platform.Contracts.V1;

namespace Platform.Application.Features.Memory.ListInsights;

public sealed class ListMemoryInsightsQueryHandler(IMemoryReadRepository memory)
{
    public async Task<IReadOnlyList<MemoryInsightDto>> HandleAsync(
        ListMemoryInsightsQuery _,
        CancellationToken cancellationToken = default) =>
        await memory.ListInsightsAsync(cancellationToken).ConfigureAwait(false);
}
