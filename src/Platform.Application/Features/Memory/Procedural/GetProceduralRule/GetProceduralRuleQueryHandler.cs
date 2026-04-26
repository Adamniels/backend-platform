using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.GetProceduralRule;

public sealed class GetProceduralRuleQueryHandler(IProceduralRuleService procedural)
{
    public async Task<ProceduralRuleDetailV1Dto?> HandleAsync(
        GetProceduralRuleQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = query.UserId is 0
            ? MemoryUser.DefaultId
            : query.UserId;
        return await procedural
            .GetDetailAsync(query.Id, userId, cancellationToken)
            .ConfigureAwait(false);
    }
}
