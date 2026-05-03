using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Configuration;
using Platform.Contracts.V1.SideLearning;
namespace Platform.Application.Features.SideLearning.Sessions.List;

public sealed class ListSideLearningSessionsQueryHandler(
    ISideLearningSessionRepository sessions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    public async Task<IReadOnlyList<SideLearningSessionSummaryV1Dto>> HandleAsync(
        ListSideLearningSessionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = workerOptions.Value.PrimaryUserId;
        var take = query.Take is < 1 or > 200 ? 50 : query.Take;
        var list = await sessions.ListForUserAsync(userId, take, cancellationToken).ConfigureAwait(false);
        return list.Select(static s => new SideLearningSessionSummaryV1Dto(
                s.Id,
                SideLearningPhaseFormatter.ToApiString(s.Phase),
                s.CreatedAt.ToString("O"),
                s.UpdatedAt.ToString("O")))
            .ToList();
    }
}
