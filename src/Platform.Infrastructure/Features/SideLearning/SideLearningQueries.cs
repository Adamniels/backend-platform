using Microsoft.EntityFrameworkCore;
using Platform.Application.Features.SideLearning;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.SideLearning;

public sealed class SideLearningQueries(PlatformDbContext db) : ISideLearningQueries
{
    public async Task<IReadOnlyList<SideLearningTopicDto>> ListTopicsAsync(CancellationToken cancellationToken = default)
    {
        return await db.SideLearningTopics.AsNoTracking()
            .OrderBy(x => x.Title)
            .Select(x => new SideLearningTopicDto(x.Id, x.Title, x.ProgressPercent))
            .ToListAsync(cancellationToken);
    }
}
