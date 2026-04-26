using FluentValidation;
using Platform.Application.Features.Memory.Context.GetMemoryContextShell;
using Platform.Contracts.V1.Memory;

namespace Platform.Application.Features.Memory.Context.GetMemoryContextV1;

public sealed class PostMemoryContextRequestHandler(
    IValidator<GetMemoryContextV1Request> validator,
    GetMemoryContextQueryHandler getContext)
{
    public async Task<MemoryContextV1Dto> HandleAsync(
        GetMemoryContextV1Request request,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken).ConfigureAwait(false);
        return await getContext
            .HandleAsync(
                new GetMemoryContextQuery(
                    request.UserId ?? 0,
                    request.TaskDescription,
                    request.WorkflowType,
                    request.ProjectId,
                    request.Domain),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
