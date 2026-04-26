using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.RejectSemanticMemory;

public sealed class RejectSemanticMemoryCommandHandler(ISemanticMemoryService semantics)
{
    public async Task<SemanticMemoryV1Dto> HandleAsync(
        RejectSemanticMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = command.UserId is 0
            ? MemoryUser.DefaultId
            : command.UserId;
        var row = await semantics
            .RejectAsync(command.SemanticMemoryId, userId, cancellationToken)
            .ConfigureAwait(false);
        return row.ToV1Dto();
    }
}
