using FluentValidation;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Features.SideLearning;
using Platform.Application.Configuration;
using Platform.Contracts.V1.SideLearning;

namespace Platform.Application.Features.SideLearning.Sessions.List;

public sealed class ListSideLearningSessionsQueryHandler(
    IValidator<ListSideLearningSessionsQuery> validator,
    ISideLearningSessionRepository sessions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    public async Task<SideLearningSessionListPageV1Dto> HandleAsync(
        ListSideLearningSessionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(validation.Errors[0].ErrorMessage);
        }

        var userId = workerOptions.Value.PrimaryUserId;
        var take = query.Take is < 1 or > 50 ? 50 : query.Take;
        var lifecycle = query.Lifecycle.Trim().ToLowerInvariant();
        var list = await sessions
            .ListForUserByLifecycleAsync(userId, lifecycle, take, cancellationToken)
            .ConfigureAwait(false);
        var items = list
            .Select(static s => new SideLearningSessionSummaryV1Dto(
                s.Id,
                SideLearningPhaseFormatter.ToApiString(s.Phase),
                s.SelectedTopicTitle,
                s.CreatedAt.ToString("O"),
                s.UpdatedAt.ToString("O")))
            .ToList();
        return new SideLearningSessionListPageV1Dto(items);
    }
}
