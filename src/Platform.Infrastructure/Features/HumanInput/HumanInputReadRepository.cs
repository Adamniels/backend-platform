using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.HumanInput;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.HumanInput;

public sealed class HumanInputReadRepository(PlatformDbContext db) : IHumanInputReadRepository
{
    public async Task<IReadOnlyList<InputNeededItemDto>> ListAsync(
        CancellationToken cancellationToken = default) =>
        await db.InputNeededItems.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new InputNeededItemDto(x.Id, x.Text, x.Type, x.Urgent, x.Detail))
            .ToListAsync(cancellationToken);
}
