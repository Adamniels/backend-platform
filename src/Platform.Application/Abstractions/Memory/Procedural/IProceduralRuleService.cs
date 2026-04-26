using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Procedural;

/// <summary>Versioned procedural rules (how agents/workflows should behave for a user).</summary>
public interface IProceduralRuleService : IProceduralRuleReadRepository
{
    Task<ProceduralRuleDetailV1Dto?> GetDetailAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>Creates version 1 as inactive, deprecates prior active rows for the same rule identity, then activates.</summary>
    Task<long> CreateAndActivateAsync(
        int userId,
        string workflowType,
        string ruleName,
        string ruleContent,
        int priority,
        string source,
        double authorityWeight,
        CancellationToken cancellationToken = default);

    /// <summary>Appends the next version for the rule family of <paramref name="basisRuleId"/>, activates it, and deprecates prior active rows.</summary>
    Task<long> PublishNewVersionActivateAsync(
        long basisRuleId,
        int userId,
        string ruleContent,
        double? authorityWeight,
        CancellationToken cancellationToken = default);

    Task<ProceduralRule> SetPriorityAsync(
        long id,
        int userId,
        int priority,
        CancellationToken cancellationToken = default);

    Task<ProceduralRule> ActivateAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default);

    Task<ProceduralRule> DeprecateAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>Applies a review-approved NewProceduralRule payload (same DbContext as the enclosing transaction).</summary>
    Task<long> ApplyApprovedNewProceduralProposalAsync(
        int userId,
        NewProceduralRuleMemoryProposalV1 payload,
        DateTimeOffset at,
        CancellationToken cancellationToken = default);
}
