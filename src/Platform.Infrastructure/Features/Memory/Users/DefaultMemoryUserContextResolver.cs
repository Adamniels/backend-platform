using Platform.Application.Abstractions.Memory.Users;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Infrastructure.Features.Memory.Users;

public sealed class DefaultMemoryUserContextResolver : IMemoryUserContextResolver
{
    public int Resolve(int requestedUserId) =>
        requestedUserId is 0 ? MemoryUser.DefaultId : requestedUserId;
}
