using Platform.Application.Abstractions.Memory.Items;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Contracts.V1.Memory;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class MemoryItemReadRepositoryStub : IMemoryItemReadRepository
{
    public Task<IReadOnlyList<MemoryItemSummaryV1Dto>> ListSummariesForUserAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<MemoryItemSummaryV1Dto>>(
            Array.Empty<MemoryItemSummaryV1Dto>());
}

public sealed class ProceduralRuleReadRepositoryStub : IProceduralRuleReadRepository
{
    public Task<IReadOnlyList<ProceduralRuleSummaryV1Dto>> ListForUserAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<ProceduralRuleSummaryV1Dto>>(
            Array.Empty<ProceduralRuleSummaryV1Dto>());
}

