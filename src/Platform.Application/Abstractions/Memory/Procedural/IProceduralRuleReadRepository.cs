using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Procedural;

public interface IProceduralRuleReadRepository
{
    Task<IReadOnlyList<ProceduralRuleSummaryV1Dto>> ListForPrincipalAsync(
        int principalId,
        CancellationToken cancellationToken = default);
}
