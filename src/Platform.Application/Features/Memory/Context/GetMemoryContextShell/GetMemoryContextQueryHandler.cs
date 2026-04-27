using Platform.Application.Abstractions.Memory.Context;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Context.GetMemoryContextShell;

public sealed class GetMemoryContextQueryHandler(
    IMemoryContextProvider provider,
    IMemoryUserContextResolver userResolver)
{
    public async Task<MemoryContextV1Dto> HandleAsync(
        GetMemoryContextQuery query,
        CancellationToken cancellationToken = default)
    {
        var id = userResolver.Resolve(query.UserId);
        var request = new MemoryContextRequest(
            id,
            query.TaskDescription,
            query.WorkflowType,
            query.ProjectId,
            query.Domain,
            query.IncludeVectorRecall);

        return await provider.GetContextAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
