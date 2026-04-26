using Platform.Contracts.V1.Memory;

namespace Platform.Application.Abstractions.Memory.Context;

public interface IMemoryContextAssembler
{
    Task<MemoryContextShellV1Dto> BuildShellAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
