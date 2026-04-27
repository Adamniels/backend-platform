using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.ListSemanticMemories;

public sealed class ListSemanticMemoriesQueryHandler(
    ISemanticMemoryService semantics,
    IMemoryUserContextResolver userResolver)
{
    public async Task<IReadOnlyList<SemanticMemoryV1Dto>> HandleAsync(
        ListSemanticMemoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(query.UserId);
        var rows = await semantics
            .ListForUserAsync(userId, query.IncludePendingReview, cancellationToken)
            .ConfigureAwait(false);
        return rows
            .Select(s => s.ToV1Dto())
            .ToList();
    }
}
