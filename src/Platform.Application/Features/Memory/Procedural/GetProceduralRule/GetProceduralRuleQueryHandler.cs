using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Procedural.GetProceduralRule;

public sealed class GetProceduralRuleQueryHandler(
    IProceduralRuleService procedural,
    IMemoryUserContextResolver userResolver)
{
    public async Task<ProceduralRuleDetailV1Dto?> HandleAsync(
        GetProceduralRuleQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(query.UserId);
        return await procedural
            .GetDetailAsync(query.Id, userId, cancellationToken)
            .ConfigureAwait(false);
    }
}
