using FluentValidation;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.DeprecateProceduralRule;

public sealed class DeprecateProceduralRuleCommandHandler(
    IValidator<DeprecateProceduralRuleCommand> validator,
    IProceduralRuleService procedural)
{
    public async Task<ProceduralRuleDetailV1Dto?> HandleAsync(
        DeprecateProceduralRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        await procedural.DeprecateAsync(command.Id, userId, cancellationToken).ConfigureAwait(false);
        return await procedural.GetDetailAsync(command.Id, userId, cancellationToken).ConfigureAwait(false);
    }
}
