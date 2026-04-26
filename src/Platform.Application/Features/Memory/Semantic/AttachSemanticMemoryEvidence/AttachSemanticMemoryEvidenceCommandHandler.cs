using FluentValidation;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.AttachSemanticMemoryEvidence;

public sealed class AttachSemanticMemoryEvidenceCommandHandler(
    IValidator<AttachSemanticMemoryEvidenceCommand> validator,
    ISemanticMemoryService semantics)
{
    public async Task<SemanticMemoryV1Dto> HandleAsync(
        AttachSemanticMemoryEvidenceCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        var row = await semantics
            .AttachEvidenceAsync(
                command.SemanticMemoryId,
                userId,
                command.EventId,
                command.Strength,
                command.Reason,
                command.FromInferredSource,
                command.ReinforceConfidence,
                command.ReinforceConfidenceDelta,
                command.EventOccurredAt,
                cancellationToken)
            .ConfigureAwait(false);
        return row.ToV1Dto();
    }
}
