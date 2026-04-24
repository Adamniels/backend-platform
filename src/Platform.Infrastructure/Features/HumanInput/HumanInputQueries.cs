using Microsoft.EntityFrameworkCore;
using Platform.Application.Features.HumanInput;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.HumanInput;

public sealed class HumanInputQueries(PlatformDbContext db) : IHumanInputQueries
{
    public async Task<IReadOnlyList<InputNeededItemDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await db.InputNeededItems.AsNoTracking()
            .OrderBy(x => x.Id)
            .Select(x => new InputNeededItemDto(x.Id, x.Text, x.Type, x.Urgent, x.Detail))
            .ToListAsync(cancellationToken);
    }
}
