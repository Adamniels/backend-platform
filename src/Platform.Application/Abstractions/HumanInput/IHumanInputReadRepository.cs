using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.HumanInput;

public interface IHumanInputReadRepository
{
    Task<IReadOnlyList<InputNeededItemDto>> ListAsync(CancellationToken cancellationToken = default);
}
