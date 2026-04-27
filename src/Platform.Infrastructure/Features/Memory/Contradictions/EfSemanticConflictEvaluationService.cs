using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions.Memory.Contradictions;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Features.Memory.Contradictions;

public sealed class EfSemanticConflictEvaluationService(
    PlatformDbContext db,
    IExplicitProfileConflictDetector explicitProfileConflictDetector) : ISemanticConflictEvaluationService
{
    public async Task<bool> ConflictsWithExplicitProfileAsync(
        int userId,
        long semanticMemoryId,
        CancellationToken cancellationToken = default)
    {
        var profile = await db.ExplicitUserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (profile is null)
        {
            return false;
        }

        var semantic = await db.SemanticMemories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == semanticMemoryId && x.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        if (semantic is null)
        {
            return false;
        }

        return explicitProfileConflictDetector.Detect(profile, [semantic]).Count > 0;
    }
}
