using FluentValidation;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Procedural.UpdateProceduralRulePriority;

public sealed class UpdateProceduralRulePriorityCommandHandler(
    IValidator<UpdateProceduralRulePriorityCommand> validator,
    IProceduralRuleService procedural)
{
    public async Task<ProceduralRuleDetailV1Dto?> HandleAsync(
        UpdateProceduralRulePriorityCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        await procedural
            .SetPriorityAsync(command.Id, userId, command.Priority, cancellationToken)
            .ConfigureAwait(false);
        return await procedural.GetDetailAsync(command.Id, userId, cancellationToken).ConfigureAwait(false);
    }
}
