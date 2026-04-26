using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Context;

/// <summary>
/// Composes the curated <b>MemoryContext</b> surface (master spec: profile, goals, semantic, episodes, rules, etc.).
/// Implementations may aggregate repositories; this port stays free of HTTP and EF.
/// </summary>
public interface IMemoryContextProvider
{
    /// <summary>Returns a versioned shell until full pipeline is implemented; call sites should not treat as exhaustive.</summary>
    Task<MemoryContextShellV1Dto> GetContextAsync(
        MemoryContextRequest request,
        CancellationToken cancellationToken = default);
}
