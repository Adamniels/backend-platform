using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.ArchiveSemanticMemory;

public sealed class ArchiveSemanticMemoryCommandHandler(
    ISemanticMemoryService semantics,
    IMemoryUserContextResolver userResolver)
{
    public async Task<SemanticMemoryV1Dto> HandleAsync(
        ArchiveSemanticMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = userResolver.Resolve(command.UserId);
        var row = await semantics
            .ArchiveAsync(command.SemanticMemoryId, userId, cancellationToken)
            .ConfigureAwait(false);
        return row.ToV1Dto();
    }
}
