using Platform.Application.Abstractions;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Health;

public sealed class EfDatabaseHealthCheck(PlatformDbContext db) : IDatabaseHealthCheck
{
    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) =>
        db.Database.CanConnectAsync(cancellationToken);
}
