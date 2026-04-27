using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Procedural.ListProceduralRules;

public sealed class ListProceduralRulesQueryHandler(
    IProceduralRuleService procedural,
    IMemoryUserContextResolver userResolver)
{
    public async Task<IReadOnlyList<ProceduralRuleSummaryV1Dto>> HandleAsync(
        ListProceduralRulesQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(query.UserId);
        return await procedural.ListForUserAsync(userId, cancellationToken).ConfigureAwait(false);
    }
}
