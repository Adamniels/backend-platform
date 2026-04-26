using Platform.Application.Abstractions.Memory.Legacy;
using Platform.Contracts.V1;

namespace Platform.Application.Features.Memory.Legacy.Insights;

public sealed class ListMemoryInsightsQueryHandler(ILegacyMemoryInsightsReadRepository memory)
{
    public async Task<IReadOnlyList<MemoryInsightDto>> HandleAsync(
        ListMemoryInsightsQuery _,
        CancellationToken cancellationToken = default) =>
        await memory.ListInsightsAsync(cancellationToken).ConfigureAwait(false);
}
