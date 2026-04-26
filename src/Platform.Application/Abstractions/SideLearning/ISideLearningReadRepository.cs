using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.SideLearning;

public interface ISideLearningReadRepository
{
    Task<IReadOnlyList<SideLearningTopicDto>> ListTopicsAsync(CancellationToken cancellationToken = default);
}
