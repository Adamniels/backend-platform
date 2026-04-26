using Platform.Api.Features.Dashboard;
using Platform.Api.Features.HumanInput;
using Platform.Api.Features.Memory;
using Platform.Api.Features.News;
using Platform.Api.Features.Profile;
using Platform.Api.Features.SavedItems;
using Platform.Api.Features.SideLearning;
using Platform.Api.Features.Stats;
using Platform.Api.Features.WorkflowRuns;

namespace Platform.Api.Features;

public static class V1ApiRegistration
{
    public static IEndpointRouteBuilder MapV1Endpoints(this IEndpointRouteBuilder app)
    {
        var v1 = app.MapGroup("/api/v1");
        DashboardV1Routes.Map(v1);
        StatsV1Routes.Map(v1);
        WorkflowRunsV1Routes.Map(v1);
        ProfileV1Routes.Map(v1);
        NewsV1Routes.Map(v1);
        SideLearningV1Routes.Map(v1);
        SavedItemsV1Routes.Map(v1);
        MemoryV1Routes.Map(v1);
        HumanInputV1Routes.Map(v1);
        return app;
    }
}
