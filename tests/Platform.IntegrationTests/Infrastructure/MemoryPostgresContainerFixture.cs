using Testcontainers.PostgreSql;
using Xunit;

namespace Platform.IntegrationTests.Infrastructure;

/// <summary>
/// Starts a disposable PostgreSQL instance (pgvector image) for memory integration tests so they never
/// touch the developer&apos;s local <c>docker compose</c> database.
/// </summary>
public sealed class MemoryPostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("pgvector/pgvector:pg17")
            .WithDatabase("platform")
            .WithUsername("platform")
            .WithPassword("platform")
            .Build();
        await _container.StartAsync().ConfigureAwait(false);
        MemoryPostgresConnection.ConnectionString = _container.GetConnectionString();
    }

    public async Task DisposeAsync()
    {
        MemoryPostgresConnection.ConnectionString = null;
        if (_container is not null)
        {
            await _container.DisposeAsync().ConfigureAwait(false);
        }
    }
}
