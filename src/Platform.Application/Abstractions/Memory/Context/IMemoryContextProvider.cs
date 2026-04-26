using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Context;

/// <summary>
/// Composes the curated <b>MemoryContext</b> surface (master spec: profile, goals, semantic, episodes, rules, etc.).
/// Implementations may aggregate repositories; this port stays free of HTTP and EF.
/// </summary>
public interface IMemoryContextProvider
{
    /// <summary>Returns a ranked v1 memory packet (SQL + deterministic scoring; not exhaustive raw tables).</summary>
    Task<MemoryContextV1Dto> GetContextAsync(
        MemoryContextRequest request,
        CancellationToken cancellationToken = default);
}
