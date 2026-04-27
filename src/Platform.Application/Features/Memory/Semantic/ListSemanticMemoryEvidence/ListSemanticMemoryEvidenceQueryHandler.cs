using Platform.Application.Abstractions.Memory.Evidence;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Semantic.ListSemanticMemoryEvidence;

public sealed class ListSemanticMemoryEvidenceQueryHandler(
    ISemanticMemoryService semantics,
    IMemoryEvidenceReadRepository evidence,
    IMemoryUserContextResolver userResolver)
{
    public async Task<IReadOnlyList<SemanticMemoryEvidenceV1Item>?> HandleAsync(
        ListSemanticMemoryEvidenceQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(query.UserId);
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
