using Platform.Application.Features.WorkflowRuns.ListWorkflowRuns;
using Platform.Application.Features.WorkflowRuns.StartWorkflowRun;
using Platform.Contracts.V1;

namespace Platform.Api.Features.WorkflowRuns;

public static class WorkflowRunsV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "workflow-runs",
            async (ListWorkflowRunsQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new ListWorkflowRunsQuery(), ct)
                        .ConfigureAwait(false)));

        v1.MapPost(
                "workflow-runs",
                async (StartWorkflowRunRequest body, StartWorkflowRunCommandHandler h, CancellationToken ct) =>
                {
                    var command = new StartWorkflowRunCommand(body.Name, body.WorkflowType, body.TaskQueue);
                    return Results.Ok(
                        await h
                            .HandleAsync(command, ct)
                            .ConfigureAwait(false));
                })
            .DisableAntiforgery();
    }
}
