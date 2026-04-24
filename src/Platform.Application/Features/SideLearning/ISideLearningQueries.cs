using Platform.Contracts.V1;

namespace Platform.Application.Features.SideLearning;

public interface ISideLearningQueries
{
    Task<IReadOnlyList<SideLearningTopicDto>> ListTopicsAsync(CancellationToken cancellationToken = default);
}
