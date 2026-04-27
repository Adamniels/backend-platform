using FluentValidation;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Procedural.DeprecateProceduralRule;

public sealed class DeprecateProceduralRuleCommandHandler(
    IValidator<DeprecateProceduralRuleCommand> validator,
    IProceduralRuleService procedural,
    IMemoryUserContextResolver userResolver)
{
    public async Task<ProceduralRuleDetailV1Dto?> HandleAsync(
        DeprecateProceduralRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = userResolver.Resolve(command.UserId);
        await procedural.DeprecateAsync(command.Id, userId, cancellationToken).ConfigureAwait(false);
        return await procedural.GetDetailAsync(command.Id, userId, cancellationToken).ConfigureAwait(false);
    }
}
