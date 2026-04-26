using FluentValidation;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.UpdateSemanticMemoryConfidence;

public sealed class UpdateSemanticMemoryConfidenceCommandHandler(
    IValidator<UpdateSemanticMemoryConfidenceCommand> validator,
    ISemanticMemoryService semantics)
{
    public async Task<SemanticMemoryV1Dto> HandleAsync(
        UpdateSemanticMemoryConfidenceCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        var row = await semantics
            .SetConfidenceAsync(
                command.SemanticMemoryId,
                userId,
                command.Confidence,
                command.FromInferredSource,
                cancellationToken)
            .ConfigureAwait(false);
        return row.ToV1Dto();
    }
}
