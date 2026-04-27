using FluentValidation;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Application.Features.Memory.Review;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.PublishProceduralRuleVersion;

public sealed class PublishProceduralRuleVersionCommandHandler(
    IValidator<PublishProceduralRuleVersionCommand> validator,
    IProceduralRuleService procedural,
    IMemoryReviewService reviews,
    IMemoryUserContextResolver userResolver)
{
    public async Task<PublishProceduralRuleVersionV1Response> HandleAsync(
        PublishProceduralRuleVersionCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = userResolver.Resolve(command.UserId);
        var detail = await procedural
            .GetDetailAsync(command.BasisRuleId, userId, cancellationToken)
            .ConfigureAwait(false);
        if (detail is null)
        {
            throw new MemoryDomainException("Procedural rule was not found for this user.");
        }

        var auth = MemoryValueConstraints.Clamp01(command.AuthorityWeight ?? detail.AuthorityWeight);
        var at = DateTimeOffset.UtcNow;
        if (ProceduralRule.ShouldQueueReviewBeforeApply(auth, command.ForceSubmitForReview))
        {
            var json = MemoryReviewProposalJson.SerializeNewProceduralRule(
                new NewProceduralRuleMemoryProposalV1
                {
                    WorkflowType = detail.WorkflowType,
                    RuleName = detail.RuleName,
                    RuleContent = command.RuleContent,
                    Priority = detail.Priority,
                    Source = detail.Source,
                    AuthorityWeight = auth,
                    BasisRuleId = command.BasisRuleId,
                });
            var item = MemoryReviewQueueItem.Propose(
                userId,
                MemoryReviewProposalType.NewProceduralRule,
                $"New version: {detail.RuleName}",
                $"Workflow «{detail.WorkflowType}» — confirm content update.",
                json,
                evidenceJson: null,
                dedupFingerprint: null,
                priority: detail.Priority,
                at);
            var saved = await reviews.CreatePendingAsync(item, cancellationToken).ConfigureAwait(false);
            return new PublishProceduralRuleVersionV1Response
            {
                Outcome = "PendingReview",
                ReviewQueueItemId = saved.Id,
            };
        }

        var id = await procedural
            .PublishNewVersionActivateAsync(
                command.BasisRuleId,
                userId,
                command.RuleContent,
                command.AuthorityWeight,
                cancellationToken)
            .ConfigureAwait(false);
        return new PublishProceduralRuleVersionV1Response { Outcome = "Activated", RuleId = id };
    }
}
