using Platform.Application.Features.Dashboard.GetSummary;

namespace Platform.Api.Features.Dashboard;

public static class DashboardV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "dashboard/summary",
            async (GetDashboardSummaryQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new GetDashboardSummaryQuery(), ct)
                        .ConfigureAwait(false)));
}
