using Platform.Application.Abstractions.Memory.Context;
using Platform.Contracts.V1.Memory;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class MemoryContextProviderShell : IMemoryContextProvider
{
    public Task<MemoryContextShellV1Dto> GetContextAsync(
        MemoryContextRequest _,
        CancellationToken __ = default) =>
        Task.FromResult(
            new MemoryContextShellV1Dto(
                "domain-shell",
                Array.Empty<MemoryItemSummaryV1Dto>(),
                Array.Empty<SemanticMemorySummaryV1Dto>()));
}
