using Platform.IntegrationTests.Infrastructure;
using Xunit;

namespace Platform.IntegrationTests;

/// <summary>
/// Serializes memory integration tests and provisions a <strong>temporary</strong> Postgres (pgvector) via
/// Testcontainers so tests do not write to the developer&apos;s local compose database.
/// </summary>
[CollectionDefinition("integration memory", DisableParallelization = true)]
public sealed class IntegrationMemoryCollection : ICollectionFixture<MemoryPostgresContainerFixture>;
