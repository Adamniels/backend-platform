using FluentValidation;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Configuration;

namespace Platform.Application.Features.SideLearning.Sessions.Delete;

public sealed class DeleteSideLearningSessionCommandHandler(
    IValidator<DeleteSideLearningSessionCommand> validator,
    ISideLearningSessionRepository sessions,
    IOptions<PlatformWorkerOptions> workerOptions)
{
    public async Task<bool> HandleAsync(
        DeleteSideLearningSessionCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = workerOptions.Value.PrimaryUserId;
        return await sessions
            .DeleteForUserAsync(command.SessionId, userId, cancellationToken)
            .ConfigureAwait(false);
    }
}
