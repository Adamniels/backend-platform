using Platform.Contracts.V1;

namespace Platform.Application.Features.SavedItems;

public interface ISavedItemQueries
{
    Task<IReadOnlyList<SavedItemSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
}
