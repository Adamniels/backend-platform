using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Context;

/// <summary>Future: compose profile, semantic, episodes, and procedural context (see <c>GetMemoryContext</c> in master doc). Structure-only for now.</summary>
public interface IMemoryContextAssembler
{
    Task<MemoryContextShellV1Dto> BuildShellAsync(
        int principalId,
        CancellationToken cancellationToken = default);
}
