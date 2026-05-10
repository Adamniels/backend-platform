using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Platform.Contracts.V1;
using Platform.Contracts.V1.News;
using Xunit;

namespace Platform.IntegrationTests;

public sealed class InternalNewsV1Tests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private const string ServiceToken = "integration-memory-worker-token";

    [Fact]
    public async Task Internal_news_items_rejects_wrong_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");
        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/items", UriKind.Relative),
            new IngestNewsItemV1Request(
                "Title",
                "https://example.com/a",
                "Test",
                "Body",
                null,
                DateTimeOffset.UtcNow.ToString("O"),
                null));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Internal_news_items_created_then_duplicate_same_url()
    {
        var url = $"https://example.com/news/{Guid.NewGuid():N}";
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);

        var body = new IngestNewsItemV1Request(
            "Integration headline",
            url,
            "Integration",
            "Summary text",
            null,
            DateTimeOffset.UtcNow.ToString("O"),
            null);

        var first = await client.PostAsJsonAsync(new Uri("/api/internal/v1/news/items", UriKind.Relative), body);
        first.EnsureSuccessStatusCode();
        var r1 = await first.Content.ReadFromJsonAsync<IngestNewsItemV1Response>();
        Assert.NotNull(r1);
        Assert.Equal("created", r1!.Status);
        Assert.False(string.IsNullOrEmpty(r1.Id));

        var second = await client.PostAsJsonAsync(new Uri("/api/internal/v1/news/items", UriKind.Relative), body);
        second.EnsureSuccessStatusCode();
        var r2 = await second.Content.ReadFromJsonAsync<IngestNewsItemV1Response>();
        Assert.NotNull(r2);
        Assert.Equal("duplicate", r2!.Status);
        Assert.Null(r2.Id);
    }

    [Fact]
    public async Task Internal_news_intelligence_runs_rejects_wrong_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");
        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/intelligence/runs", UriKind.Relative),
            new TriggerNewsIntelligenceWorkflowV1Request(null));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Internal_news_intelligence_runs_returns_workflow_run_summary()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);

        await client.PostAsJsonAsync(new Uri("/api/admin/unlock", UriKind.Relative), new UnlockRequest("integration-test-access-key"));

        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/news/intelligence/runs", UriKind.Relative),
            new TriggerNewsIntelligenceWorkflowV1Request("News intelligence (integration)"));
        res.EnsureSuccessStatusCode();
        var dto = await res.Content.ReadFromJsonAsync<WorkflowRunSummaryDto>();
        Assert.NotNull(dto);
        Assert.StartsWith("wr-", dto!.Id, StringComparison.Ordinal);
        Assert.Equal("News intelligence (integration)", dto.Name);
        Assert.NotEmpty(dto.Status);
    }
}
