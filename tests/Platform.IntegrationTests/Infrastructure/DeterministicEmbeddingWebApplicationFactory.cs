using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Application.Features.Memory.Embeddings;

namespace Platform.IntegrationTests.Infrastructure;

/// <summary>
/// Extends <see cref="PlatformWebApplicationFactory"/> by replacing the singleton
/// <see cref="IMemoryEmbeddingGenerator"/> with the deterministic stub, so integration
/// tests that exercise embedding-dependent handlers do not require an OpenAI API key.
/// Also stubs <see cref="IUserInterestProvider"/> so profile-seed tests always have
/// non-empty content to embed regardless of whether the database has memory data.
/// </summary>
public sealed class DeterministicEmbeddingWebApplicationFactory : PlatformWebApplicationFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Replace the OpenAI-backed singleton with the deterministic local generator.
            var embeddingDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMemoryEmbeddingGenerator));
            if (embeddingDescriptor is not null)
                services.Remove(embeddingDescriptor);

            services.AddSingleton<IMemoryEmbeddingGenerator, DeterministicRecallEmbeddingGenerator>();

            // Replace IUserInterestProvider so seed tests don't depend on memory profile data.
            var interestDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IUserInterestProvider));
            if (interestDescriptor is not null)
                services.Remove(interestDescriptor);

            services.AddSingleton<IUserInterestProvider, StubUserInterestProvider>();
        });
    }

    /// <summary>
    /// Returns a fixed non-empty snapshot so <see cref="Platform.Application.Features.News.Profile.SeedNewsProfileCommandHandler"/>
    /// always has content to embed without needing live memory profile data.
    /// </summary>
    private sealed class StubUserInterestProvider : IUserInterestProvider
    {
        private static readonly UserInterestSnapshot Snapshot = new(
            ["software engineering", "AI", "platform infrastructure"],
            ["developer productivity"],
            ["ship reliable software"],
            []);

        public Task<UserInterestSnapshot> GetInterestsAsync(int userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Snapshot);
    }
}
