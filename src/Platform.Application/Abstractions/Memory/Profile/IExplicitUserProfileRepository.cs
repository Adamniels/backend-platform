using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Abstractions.Memory.Profile;

/// <summary>
/// User-entered profile memory: one row per user, highest authority (1.0). Inferred pipelines must not use this;
/// they should write inferred <see cref="MemoryItem" /> or <see cref="SemanticMemory" /> rows instead.
/// </summary>
public interface IExplicitUserProfileRepository
{
    Task<ExplicitUserProfile?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<ExplicitUserProfile> SaveAsync(
        ExplicitUserProfile model,
        CancellationToken cancellationToken = default);
}
