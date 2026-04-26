using Platform.Contracts.V1;

namespace Platform.Application.Abstractions.SavedItems;

public interface ISavedItemsReadRepository
{
    Task<IReadOnlyList<SavedItemSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
}
