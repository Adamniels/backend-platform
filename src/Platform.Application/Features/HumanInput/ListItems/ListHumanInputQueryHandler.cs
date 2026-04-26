using Platform.Application.Abstractions.HumanInput;
using Platform.Contracts.V1;

namespace Platform.Application.Features.HumanInput.ListItems;

public sealed class ListHumanInputQueryHandler(IHumanInputReadRepository items)
{
    public async Task<IReadOnlyList<InputNeededItemDto>> HandleAsync(
        ListHumanInputQuery _,
        CancellationToken cancellationToken = default) =>
        await items.ListAsync(cancellationToken).ConfigureAwait(false);
}
