using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.SideLearning;
using Platform.Contracts.V1;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.SideLearning;

public sealed class SideLearningReadRepository(PlatformDbContext db) : ISideLearningReadRepository
{
    public async Task<IReadOnlyList<SideLearningTopicDto>> ListTopicsAsync(
        CancellationToken cancellationToken = default) =>
        await db.SideLearningTopics.AsNoTracking()
            .OrderBy(x => x.Title)
            .Select(x => new SideLearningTopicDto(x.Id, x.Title, x.ProgressPercent))
            .ToListAsync(cancellationToken);
}
