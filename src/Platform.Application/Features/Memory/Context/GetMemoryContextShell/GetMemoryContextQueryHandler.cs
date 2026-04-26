using Platform.Application.Abstractions.Memory.Context;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Context.GetMemoryContextShell;

public sealed class GetMemoryContextQueryHandler(IMemoryContextAssembler asm)
{
    public async Task<MemoryContextShellV1Dto> HandleAsync(
        GetMemoryContextQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = query.UserId is 0 ? MemoryUser.DefaultId : query.UserId;
        return await asm.BuildShellAsync(id, cancellationToken).ConfigureAwait(false);
    }
}
