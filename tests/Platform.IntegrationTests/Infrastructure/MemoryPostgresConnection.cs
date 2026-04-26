namespace Platform.IntegrationTests.Infrastructure;

/// <summary>
/// Holds the connection string for the ephemeral Postgres used by the
/// <see cref="IntegrationMemoryCollection"/> Testcontainer. Empty when not running that collection.
/// </summary>
public static class MemoryPostgresConnection
{
    public static string? ConnectionString { get; set; }
}
