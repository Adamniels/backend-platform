using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Stats;
using Platform.Contracts.V1;
using Platform.Domain.Features.Dashboard;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Dashboard;

public sealed class StatsReadModelSource(PlatformDbContext db) : IStatsReadModelSource
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<StatsPayloadDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var row = await db.StatsSnapshots.AsNoTracking()
            .SingleAsync(x => x.Id == StatsSnapshot.SingletonKey, cancellationToken);
        return JsonSerializer.Deserialize<StatsPayloadDto>(row.Json, JsonOptions) ?? new StatsPayloadDto([], [], []);
    }
}
