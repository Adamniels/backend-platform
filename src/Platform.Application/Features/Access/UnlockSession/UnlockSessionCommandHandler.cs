using Platform.Application.Abstractions.Access;

namespace Platform.Application.Features.Access.UnlockSession;

public sealed class UnlockSessionCommandHandler(IAccessKeyValidationService access)
{
    public Task<UnlockSessionOutcome> HandleAsync(UnlockSessionCommand command, CancellationToken _ = default) =>
        Task.FromResult(access.ValidateAccessKey(command.AccessKey));
}
