using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Platform.Contracts.V1;
using Platform.Contracts.V1.News;
using Platform.IntegrationTests.Infrastructure;
using Xunit;

namespace Platform.IntegrationTests;

/// <summary>
/// Uses <see cref="DeterministicEmbeddingWebApplicationFactory"/> so items can be embedded
/// before the feed is checked. This ensures the feed assertion is robust whether or not
/// user 1 has a news profile and vector search is active.
/// </summary>
public sealed class NewsV1DeleteTests(DeterministicEmbeddingWebApplicationFactory factory)
    : IClassFixture<DeterministicEmbeddingWebApplicationFactory>
{
    private const string ServiceToken = "integration-memory-worker-token";
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Delete_news_items_requires_session()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var res = await client.PostAsJsonAsync(
            new Uri("/api/v1/news/items/delete", UriKind.Relative),
            new DeleteNewsItemsV1Request(["ni-nonexistent"]));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Delete_news_items_batch_removes_from_feed()
    {
        var url1 = $"https://example.com/del-{Guid.NewGuid():N}";
        var url2 = $"https://example.com/del-{Guid.NewGuid():N}";

        using var internalClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        internalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);

        async Task<string> Ingest(string url, string title)
        {
            var body = new IngestNewsItemV1Request(
                title,
                url,
                "IntegrationDelete",
                "Body text for embedding",
                null,
                DateTimeOffset.UtcNow.ToString("O"),
                null);
            var r = await internalClient.PostAsJsonAsync(
                new Uri("/api/internal/v1/news/items", UriKind.Relative),
                body);
            r.EnsureSuccessStatusCode();
            var json = await r.Content.ReadFromJsonAsync<IngestNewsItemV1Response>(JsonReadOptions);
            Assert.NotNull(json);
            Assert.Equal("created", json!.Status);
            Assert.False(string.IsNullOrEmpty(json.Id));
            return json.Id!;
        }

        async Task Embed(string id)
        {
            var r = await internalClient.PostAsync(
                new Uri($"/api/internal/v1/news/items/{id}/embed", UriKind.Relative),
                null);
            r.EnsureSuccessStatusCode();
        }

        var id1 = await Ingest(url1, "Delete test A");
        var id2 = await Ingest(url2, "Delete test B");

        // Embed both items so they appear in the feed regardless of whether vector
        // search is active for user 1 (robust against stale profile data in the DB).
        await Embed(id1);
        await Embed(id2);

        using var userClient = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await userClient.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var feedBefore = await userClient.GetFromJsonAsync<List<NewsItemSummaryDto>>(
            new Uri("/api/v1/news/feed", UriKind.Relative),
            JsonReadOptions);
        Assert.NotNull(feedBefore);
        Assert.Contains(feedBefore!, i => i.Id == id1);
        Assert.Contains(feedBefore!, i => i.Id == id2);

        var del = await userClient.PostAsJsonAsync(
            new Uri("/api/v1/news/items/delete", UriKind.Relative),
            new DeleteNewsItemsV1Request([id1, id2]));
        del.EnsureSuccessStatusCode();
        var delBody = await del.Content.ReadFromJsonAsync<DeleteNewsItemsV1Response>(JsonReadOptions);
        Assert.NotNull(delBody);
        Assert.Equal(2, delBody!.Deleted);

        var feedAfter = await userClient.GetFromJsonAsync<List<NewsItemSummaryDto>>(
            new Uri("/api/v1/news/feed", UriKind.Relative),
            JsonReadOptions);
        Assert.NotNull(feedAfter);
        Assert.DoesNotContain(feedAfter!, i => i.Id == id1);
        Assert.DoesNotContain(feedAfter!, i => i.Id == id2);
    }
}
