using Platform.Application.Abstractions.Memory.Evidence;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Semantic.ListSemanticMemoryEvidence;

public sealed class ListSemanticMemoryEvidenceQueryHandler(
    ISemanticMemoryService semantics,
    IMemoryEvidenceReadRepository evidence)
{
    public async Task<IReadOnlyList<SemanticMemoryEvidenceV1Item>?> HandleAsync(
        ListSemanticMemoryEvidenceQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = query.UserId is 0 ? MemoryUser.DefaultId : query.UserId;
        var sm = await semantics
            .GetByIdAsync(query.SemanticMemoryId, userId, cancellationToken)
            .ConfigureAwait(false);
        if (sm is null)
        {
            return null;
        }

        return await evidence
            .ListForSemanticAsync(userId, query.SemanticMemoryId, query.Take, cancellationToken)
            .ConfigureAwait(false);
    }
}
