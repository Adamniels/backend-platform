namespace Platform.Application.Abstractions.Memory.Users;

public interface IMemoryUserContextResolver
{
    int Resolve(int requestedUserId);
}
