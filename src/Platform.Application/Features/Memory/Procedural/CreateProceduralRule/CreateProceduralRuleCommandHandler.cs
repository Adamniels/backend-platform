using FluentValidation;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Application.Features.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.CreateProceduralRule;

public sealed class CreateProceduralRuleCommandHandler(
    IValidator<CreateProceduralRuleCommand> validator,
    IProceduralRuleService procedural,
    IMemoryReviewService reviews,
    IMemoryUserContextResolver userResolver)
{
    public async Task<CreateProceduralRuleV1Response> HandleAsync(
        CreateProceduralRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = userResolver.Resolve(command.UserId);
        var auth = MemoryValueConstraints.Clamp01(command.AuthorityWeight);
        var at = DateTimeOffset.UtcNow;
        if (ProceduralRule.ShouldQueueReviewBeforeApply(auth, command.ForceSubmitForReview))
        {
            var json = MemoryReviewProposalJson.SerializeNewProceduralRule(
                new NewProceduralRuleMemoryProposalV1
                {
                    WorkflowType = command.WorkflowType.Trim(),
                    RuleName = command.RuleName.Trim(),
                    RuleContent = command.RuleContent ?? "",
                    Priority = command.Priority,
                    Source = command.Source.Trim(),
                    AuthorityWeight = auth,
                    BasisRuleId = null,
                });
            var item = MemoryReviewQueueItem.Propose(
                userId,
                MemoryReviewProposalType.NewProceduralRule,
                $"Procedural rule: {command.RuleName.Trim()}",
                $"Workflow «{command.WorkflowType.Trim()}» — confirm before activation.",
                json,
                evidenceJson: null,
                dedupFingerprint: null,
                priority: command.Priority,
                at);
            var saved = await reviews.CreatePendingAsync(item, cancellationToken).ConfigureAwait(false);
            return new CreateProceduralRuleV1Response
            {
                Outcome = "PendingReview",
                ReviewQueueItemId = saved.Id,
            };
        }

        var id = await procedural
            .CreateAndActivateAsync(
                userId,
                command.WorkflowType,
                command.RuleName,
                command.RuleContent,
                command.Priority,
                command.Source,
                auth,
                cancellationToken)
            .ConfigureAwait(false);
        return new CreateProceduralRuleV1Response { Outcome = "Activated", RuleId = id };
    }
}
