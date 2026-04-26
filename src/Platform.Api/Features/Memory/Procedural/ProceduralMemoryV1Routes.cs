using Microsoft.AspNetCore.Http;
using Platform.Application.Features.Memory.Procedural.ActivateProceduralRule;
using Platform.Application.Features.Memory.Procedural.CreateProceduralRule;
using Platform.Application.Features.Memory.Procedural.DeprecateProceduralRule;
using Platform.Application.Features.Memory.Procedural.GetProceduralRule;
using Platform.Application.Features.Memory.Procedural.ListProceduralRules;
using Platform.Application.Features.Memory.Procedural.PublishProceduralRuleVersion;
using Platform.Application.Features.Memory.Procedural.UpdateProceduralRulePriority;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Procedural;

public static class ProceduralMemoryV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "memory/procedural-rules",
            async (int? userId, ListProceduralRulesQueryHandler h, CancellationToken ct) =>
            {
                var q = new ListProceduralRulesQuery(userId ?? 0);
                return Results.Ok(await h.HandleAsync(q, ct).ConfigureAwait(false));
            });

        v1.MapGet(
            "memory/procedural-rules/{id:long}",
            async (long id, int? userId, GetProceduralRuleQueryHandler h, CancellationToken ct) =>
            {
                var res = await h
                    .HandleAsync(new GetProceduralRuleQuery(id, userId ?? 0), ct)
                    .ConfigureAwait(false);
                return res is null
                    ? Results.NotFound()
                    : Results.Ok(res);
            });

        v1.MapPost(
                "memory/procedural-rules",
                async (CreateProceduralRuleV1Request body, CreateProceduralRuleCommandHandler h, CancellationToken ct) =>
                {
                    var cmd = new CreateProceduralRuleCommand(
                        body.UserId ?? 0,
                        body.WorkflowType,
                        body.RuleName,
                        body.RuleContent,
                        body.Priority,
                        body.Source,
                        body.AuthorityWeight,
                        body.ForceSubmitForReview);
                    var res = await h.HandleAsync(cmd, ct).ConfigureAwait(false);
                    return Results.Json(res, statusCode: StatusCodes.Status201Created);
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/procedural-rules/{id:long}/versions",
                async (
                    long id,
                    PublishProceduralRuleVersionV1Request body,
                    PublishProceduralRuleVersionCommandHandler h,
                    CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(
                            new PublishProceduralRuleVersionCommand(
                                id,
                                body.UserId ?? 0,
                                body.RuleContent,
                                body.AuthorityWeight,
                                body.ForceSubmitForReview),
                            ct)
                        .ConfigureAwait(false);
                    return Results.Json(res, statusCode: StatusCodes.Status201Created);
                })
            .DisableAntiforgery();

        v1.MapPut(
                "memory/procedural-rules/{id:long}/priority",
                async (
                    long id,
                    UpdateProceduralRulePriorityV1Request body,
                    UpdateProceduralRulePriorityCommandHandler h,
                    CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(
                            new UpdateProceduralRulePriorityCommand(id, body.UserId ?? 0, body.Priority),
                            ct)
                        .ConfigureAwait(false);
                    return res is null
                        ? Results.NotFound()
                        : Results.Ok(res);
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/procedural-rules/{id:long}/activate",
                async (long id, int? userId, ActivateProceduralRuleCommandHandler h, CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(new ActivateProceduralRuleCommand(id, userId ?? 0), ct)
                        .ConfigureAwait(false);
                    return res is null
                        ? Results.NotFound()
                        : Results.Ok(res);
                })
            .DisableAntiforgery();

        v1.MapPost(
                "memory/procedural-rules/{id:long}/deprecate",
                async (long id, int? userId, DeprecateProceduralRuleCommandHandler h, CancellationToken ct) =>
                {
                    var res = await h
                        .HandleAsync(new DeprecateProceduralRuleCommand(id, userId ?? 0), ct)
                        .ConfigureAwait(false);
                    return res is null
                        ? Results.NotFound()
                        : Results.Ok(res);
                })
            .DisableAntiforgery();
    }
}
