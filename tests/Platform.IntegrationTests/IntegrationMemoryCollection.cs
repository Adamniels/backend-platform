namespace Platform.IntegrationTests;

/// <summary>Serializes tests that mutate the same single-tenant memory profile row (user id 1).</summary>
[CollectionDefinition("integration memory", DisableParallelization = true)]
public sealed class IntegrationMemoryCollection;
