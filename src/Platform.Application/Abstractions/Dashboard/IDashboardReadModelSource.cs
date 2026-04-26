using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.Dashboard;

public interface IDashboardReadModelSource
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
