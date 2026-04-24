using Platform.Contracts.V1;

namespace Platform.Application.Features.HumanInput;

public interface IHumanInputQueries
{
    Task<IReadOnlyList<InputNeededItemDto>> ListAsync(CancellationToken cancellationToken = default);
}
