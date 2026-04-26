using Platform.Application.Abstractions.Memory.Context;
using Platform.Contracts.V1.Memory;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class MemoryContextAssemblerShell : IMemoryContextAssembler
{
    public Task<MemoryContextShellV1Dto> BuildShellAsync(int _, CancellationToken __ = default) =>
        Task.FromResult(
            new MemoryContextShellV1Dto("structure-only", Array.Empty<MemoryItemSummaryV1Dto>(),
                Array.Empty<SemanticMemorySummaryV1Dto>()));
}
