using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class SemanticMemoryServiceShell : ISemanticMemoryService
{
    public Task<IReadOnlyList<SemanticMemory>> ListActiveAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<SemanticMemory>>(
            Array.Empty<SemanticMemory>());

    public Task<SemanticMemory?> GetByIdAsync(
        long _1,
        int _2,
        CancellationToken _3 = default) =>
        Task.FromResult<SemanticMemory?>(null);
}
