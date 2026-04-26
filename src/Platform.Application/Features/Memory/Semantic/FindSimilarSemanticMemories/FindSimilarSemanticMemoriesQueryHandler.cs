using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.FindSimilarSemanticMemories;

public sealed class FindSimilarSemanticMemoriesQueryHandler(ISemanticMemoryService semantics)
{
    public async Task<IReadOnlyList<SemanticMemoryV1Dto>> HandleAsync(
        FindSimilarSemanticMemoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = query.UserId is 0
            ? MemoryUser.DefaultId
            : query.UserId;
        var rows = await semantics
            .FindSimilarByKeyOrDomainAsync(
                userId,
                query.KeySubstring,
                query.Domain,
                query.Take,
                cancellationToken)
            .ConfigureAwait(false);
        return rows
            .Select(s => s.ToV1Dto())
            .ToList();
    }
}
