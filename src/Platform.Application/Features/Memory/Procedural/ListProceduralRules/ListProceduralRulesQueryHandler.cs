using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.ListProceduralRules;

public sealed class ListProceduralRulesQueryHandler(IProceduralRuleService procedural)
{
    public async Task<IReadOnlyList<ProceduralRuleSummaryV1Dto>> HandleAsync(
        ListProceduralRulesQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = query.UserId is 0
            ? MemoryUser.DefaultId
            : query.UserId;
        return await procedural.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}
