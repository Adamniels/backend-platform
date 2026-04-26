using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Semantic;

/// <summary>Learned claims with confidence and evidence links; no ML in this port—persistence and domain rules only.</summary>
public interface ISemanticMemoryService
{
    Task<IReadOnlyList<SemanticMemory>> ListActiveAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<SemanticMemory?> GetByIdAsync(
        long id,
        int userId,
        CancellationToken cancellationToken = default);
}
