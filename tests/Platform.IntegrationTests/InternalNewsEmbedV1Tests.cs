using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.V1.News;
using Platform.IntegrationTests.Infrastructure;
using Xunit;

namespace Platform.IntegrationTests;

/// <summary>
/// Integration tests for POST /api/internal/v1/news/items/{id}/embed.
/// Uses <see cref="DeterministicEmbeddingWebApplicationFactory"/> so the OpenAI API
/// key is not required — the deterministic generator produces real pgvector vectors.
/// </summary>
public sealed class InternalNewsEmbedV1Tests(DeterministicEmbeddingWebApplicationFactory factory)
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
    public async Task Embed_returns_unauthorised_for_wrong_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");

        var res = await client.PostAsync(
            new Uri("/api/internal/v1/news/items/ni-" + new string('a', 32) + "/embed", UriKind.Relative),
            null);

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Embed_returns_error_for_missing_news_item()
    {
        using var client = AuthorizedClient();

        // A well-formed id that does not exist in the database.
        var missingId = "ni-" + new string('f', 32);
        var res = await client.PostAsync(
            new Uri($"/api/internal/v1/news/items/{missingId}/embed", UriKind.Relative),
            null);

        res.EnsureSuccessStatusCode();
        var dto = await res.Content.ReadFromJsonAsync<EmbedNewsItemV1Response>();
        Assert.NotNull(dto);
        Assert.Equal("error", dto!.Status);
    }

    [Fact]
    public async Task Embed_returns_embedded_then_skipped_for_same_item()
    {
        using var ingestClient = AuthorizedClient();

        // Ingest a fresh news item so we have a real id to embed.
        var url = $"https://example.com/embed-test/{Guid.NewGuid():N}";
        var ingestBody = new IngestNewsItemV1Request(
            "Embed test headline",
            url,
            "EmbedSource",
            "This is the article body used for embedding.",
            null,
            DateTimeOffset.UtcNow.ToString("O"),
            null);

        var ingestRes = await ingestClient.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/items", UriKind.Relative),
            ingestBody);
        ingestRes.EnsureSuccessStatusCode();
        var ingestDto = await ingestRes.Content.ReadFromJsonAsync<IngestNewsItemV1Response>();
        Assert.NotNull(ingestDto);
        Assert.Equal("created", ingestDto!.Status);
        var newsId = ingestDto.Id!;

        using var embedClient = AuthorizedClient();

        // First embed — should produce "embedded".
        var first = await embedClient.PostAsync(
            new Uri($"/api/internal/v1/news/items/{newsId}/embed", UriKind.Relative),
            null);
        first.EnsureSuccessStatusCode();
        var firstDto = await first.Content.ReadFromJsonAsync<EmbedNewsItemV1Response>();
        Assert.NotNull(firstDto);
        Assert.Equal("embedded", firstDto!.Status);

        // Second embed — same model key, should produce "skipped".
        var second = await embedClient.PostAsync(
            new Uri($"/api/internal/v1/news/items/{newsId}/embed", UriKind.Relative),
            null);
        second.EnsureSuccessStatusCode();
        var secondDto = await second.Content.ReadFromJsonAsync<EmbedNewsItemV1Response>();
        Assert.NotNull(secondDto);
        Assert.Equal("skipped", secondDto!.Status);
    }

    [Fact]
    public async Task Embed_returns_bad_request_for_invalid_id_format()
    {
        using var client = AuthorizedClient();

        var res = await client.PostAsync(
            new Uri("/api/internal/v1/news/items/not-a-valid-id/embed", UriKind.Relative),
            null);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }
}
