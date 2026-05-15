using Microsoft.EntityFrameworkCore;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Persistence;

namespace Platform.Infrastructure.Health;

public sealed class EfDatabaseMigrator(PlatformDbContext db) : IDatabaseMigrator
{
    public Task MigrateAsync(CancellationToken cancellationToken = default) =>
        db.Database.MigrateAsync(cancellationToken);
}
