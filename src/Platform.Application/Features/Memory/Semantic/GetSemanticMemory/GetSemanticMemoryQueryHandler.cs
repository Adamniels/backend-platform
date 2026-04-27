using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.GetSemanticMemory;

public sealed class GetSemanticMemoryQueryHandler(
    ISemanticMemoryService semantics,
    IMemoryUserContextResolver userResolver)
{
    public async Task<SemanticMemoryV1Dto?> HandleAsync(
        GetSemanticMemoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(query.UserId);
        var row = await semantics
            .GetByIdAsync(query.Id, userId, cancellationToken)
            .ConfigureAwait(false);
        return row is null
            ? null
            : row.ToV1Dto();
    }
}
