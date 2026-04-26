using Platform.Application.Abstractions.Memory.Items;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Contracts.V1.Memory;

namespace Platform.Infrastructure.Features.Memory.Stubs;

public sealed class MemoryItemReadRepositoryStub : IMemoryItemReadRepository
{
    public Task<IReadOnlyList<MemoryItemSummaryV1Dto>> ListSummariesForPrincipalAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<MemoryItemSummaryV1Dto>>(
            Array.Empty<MemoryItemSummaryV1Dto>());
}

public sealed class SemanticMemoryReadRepositoryStub : ISemanticMemoryReadRepository
{
    public Task<IReadOnlyList<SemanticMemorySummaryV1Dto>> ListSummariesForPrincipalAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<SemanticMemorySummaryV1Dto>>(
            Array.Empty<SemanticMemorySummaryV1Dto>());
}

public sealed class ProceduralRuleReadRepositoryStub : IProceduralRuleReadRepository
{
    public Task<IReadOnlyList<ProceduralRuleSummaryV1Dto>> ListForPrincipalAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<ProceduralRuleSummaryV1Dto>>(
            Array.Empty<ProceduralRuleSummaryV1Dto>());
}

public sealed class MemoryReviewQueueReadRepositoryStub : IMemoryReviewQueueReadRepository
{
    public Task<IReadOnlyList<MemoryReviewQueueItemV1Dto>> ListPendingForPrincipalAsync(
        int _,
        CancellationToken __ = default) =>
        Task.FromResult<IReadOnlyList<MemoryReviewQueueItemV1Dto>>(
            Array.Empty<MemoryReviewQueueItemV1Dto>());
}
