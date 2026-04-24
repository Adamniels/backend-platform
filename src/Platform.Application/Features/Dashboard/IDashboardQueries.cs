using Platform.Contracts.V1;

namespace Platform.Application.Features.Dashboard;

public interface IDashboardQueries
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
