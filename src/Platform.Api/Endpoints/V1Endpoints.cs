using Platform.Application.Features.Dashboard;
using Platform.Application.Features.HumanInput;
using Platform.Application.Features.Memory;
using Platform.Application.Features.News;
using Platform.Application.Features.Profile;
using Platform.Application.Features.SavedItems;
using Platform.Application.Features.SideLearning;
using Platform.Application.Features.WorkflowRuns;
using Platform.Contracts.V1;

namespace Platform.Api.Endpoints;

public static class V1Endpoints
{
    public static IEndpointRouteBuilder MapV1Endpoints(this IEndpointRouteBuilder app)
    {
        var v1 = app.MapGroup("/api/v1");

        v1.MapGet("/dashboard/summary", async (IDashboardQueries q, CancellationToken ct) => Results.Ok(await q.GetSummaryAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/stats", async (IStatsQueries q, CancellationToken ct) => Results.Ok(await q.GetAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/workflow-runs", async (IWorkflowRunQueries q, CancellationToken ct) => Results.Ok(await q.ListAsync(ct).ConfigureAwait(false)));

        v1.MapPost(
                "/workflow-runs",
                async (StartWorkflowRunRequest body, IWorkflowRunCommands commands, CancellationToken ct) =>
                    Results.Ok(await commands.StartAsync(body.Name, body.WorkflowType, body.TaskQueue ?? "", ct).ConfigureAwait(false)))
            .DisableAntiforgery();

        v1.MapGet("/profile", async (IProfileQueries q, CancellationToken ct) => Results.Ok(await q.GetProfileAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/settings", async (IProfileQueries q, CancellationToken ct) => Results.Ok(await q.GetSettingsAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/news/feed", async (INewsQueries q, CancellationToken ct) => Results.Ok(await q.ListFeedAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/side-learning/topics", async (ISideLearningQueries q, CancellationToken ct) => Results.Ok(await q.ListTopicsAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/saved-items", async (ISavedItemQueries q, CancellationToken ct) => Results.Ok(await q.ListAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/memory/insights", async (IMemoryQueries q, CancellationToken ct) => Results.Ok(await q.ListInsightsAsync(ct).ConfigureAwait(false)));

        v1.MapGet("/human-input/items", async (IHumanInputQueries q, CancellationToken ct) => Results.Ok(await q.ListAsync(ct).ConfigureAwait(false)));

        return app;
    }
}
