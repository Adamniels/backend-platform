using Platform.Application.Abstractions.SideLearning;
using Platform.Contracts.V1;

namespace Platform.Application.Features.SideLearning.ListTopics;

public sealed class ListSideLearningTopicsQueryHandler(ISideLearningReadRepository source)
{
    public async Task<IReadOnlyList<SideLearningTopicDto>> HandleAsync(
        ListSideLearningTopicsQuery _,
        CancellationToken cancellationToken = default) =>
        await source.ListTopicsAsync(cancellationToken).ConfigureAwait(false);
}
