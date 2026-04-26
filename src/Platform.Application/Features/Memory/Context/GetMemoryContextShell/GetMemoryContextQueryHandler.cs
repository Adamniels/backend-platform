using Platform.Application.Abstractions.Memory.Context;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Context.GetMemoryContextShell;

public sealed class GetMemoryContextQueryHandler(IMemoryContextProvider provider)
{
    public async Task<MemoryContextV1Dto> HandleAsync(
        GetMemoryContextQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = query.UserId is 0 ? MemoryUser.DefaultId : query.UserId;
        var request = new MemoryContextRequest(
            id,
            query.TaskDescription,
            query.WorkflowType,
            query.ProjectId,
            query.Domain);

        return await provider.GetContextAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
