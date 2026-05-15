using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.V1.News;
using Platform.IntegrationTests.Infrastructure;
using Xunit;

namespace Platform.IntegrationTests;

/// <summary>
/// Integration tests for POST /api/internal/v1/news/profile/seed.
/// Uses <see cref="DeterministicEmbeddingWebApplicationFactory"/> so the OpenAI API
/// key is not required — the deterministic generator produces real pgvector vectors.
/// </summary>
public sealed class InternalNewsProfileV1Tests(DeterministicEmbeddingWebApplicationFactory factory)
    : IClassFixture<DeterministicEmbeddingWebApplicationFactory>
{
    private const string ServiceToken = "integration-memory-worker-token";

    private HttpClient AuthorizedClient()
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);
        return client;
    }

    [Fact]
    public async Task Seed_returns_unauthorised_for_wrong_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");

        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/profile/seed", UriKind.Relative),
            new SeedNewsProfileV1Request(1));

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Seed_returns_seeded_on_first_call_and_exists_on_repeat()
    {
        // Use a user ID that is unlikely to collide with other test runs.
        // Profile seed is idempotent so repeat calls are safe.
        const int userId = 9_001;

        using var client = AuthorizedClient();

        // First call — profile should not exist yet → "seeded".
        // (If prior test run left a profile, this returns "exists" which is also acceptable.)
        var first = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/profile/seed", UriKind.Relative),
            new SeedNewsProfileV1Request(userId));
        first.EnsureSuccessStatusCode();
        var firstDto = await first.Content.ReadFromJsonAsync<SeedNewsProfileV1Response>();
        Assert.NotNull(firstDto);
        Assert.True(
            firstDto!.Status is "seeded" or "exists",
            $"Expected 'seeded' or 'exists' but got '{firstDto.Status}'");

        // Second call — profile now exists → "exists".
        var second = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/profile/seed", UriKind.Relative),
            new SeedNewsProfileV1Request(userId));
        second.EnsureSuccessStatusCode();
        var secondDto = await second.Content.ReadFromJsonAsync<SeedNewsProfileV1Response>();
        Assert.NotNull(secondDto);
        Assert.Equal("exists", secondDto!.Status);
    }

    [Fact]
    public async Task Seed_returns_bad_request_for_zero_user_id()
    {
        using var client = AuthorizedClient();

        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/profile/seed", UriKind.Relative),
            new SeedNewsProfileV1Request(0));

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
