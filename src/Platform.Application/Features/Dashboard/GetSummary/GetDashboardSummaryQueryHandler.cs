using Platform.Application.Abstractions.Dashboard;
using Platform.Contracts.V1;

namespace Platform.Application.Features.Dashboard.GetSummary;

public sealed class GetDashboardSummaryQueryHandler(IDashboardReadModelSource source)
{
    public async Task<DashboardSummaryDto> HandleAsync(
        GetDashboardSummaryQuery _,
        CancellationToken cancellationToken = default) =>
        await source.GetSummaryAsync(cancellationToken).ConfigureAwait(false);
}
