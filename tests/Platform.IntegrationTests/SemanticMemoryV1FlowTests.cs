using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class SemanticMemoryV1FlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Semantic_create_list_find_archive_honors_evidence_and_dedup()
    {
        var k = $"sem-key-{Guid.NewGuid():N}";
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            });

        await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var evRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/events", UriKind.Relative),
            new IngestMemoryEventV1Request
            {
                EventType = "integration.semantic",
                UserId = 1,
                OccurredAt = DateTimeOffset.UtcNow,
                PayloadJson = "{\"n\":1}",
            });
        evRes.EnsureSuccessStatusCode();
        var ev = await evRes.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);
        Assert.NotNull(ev);
        var eventId = ev!.Id;

        var create = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/semantics", UriKind.Relative),
            new CreateSemanticMemoryV1Request
            {
                UserId = 1,
                Key = k,
                Claim = "user prefers strong typing",
                Confidence = 0.7d,
                Domain = "work",
                Status = "Active",
                EventId = eventId,
                EvidenceStrength = 0.9d,
                EvidenceReason = "integration",
            });
        create.EnsureSuccessStatusCode();
        var sem = await create.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(sem);
        Assert.Equal(k, sem!.Key);

        var dup = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/semantics", UriKind.Relative),
            new CreateSemanticMemoryV1Request
            {
                UserId = 1,
                Key = k,
                Claim = "other",
                Confidence = 0.5d,
                Domain = "work",
                EventId = eventId,
                EvidenceStrength = 0.5d,
            });
        Assert.Equal(HttpStatusCode.Conflict, dup.StatusCode);

        var list = await client.GetAsync(new Uri($"/api/v1/memory/semantics?userId=1&includePending=true", UriKind.Relative));
        list.EnsureSuccessStatusCode();
        var listBody = await list.Content.ReadFromJsonAsync<SemanticMemoryV1Dto[]>(JsonReadOptions);
        Assert.NotNull(listBody);
        Assert.Contains(
            listBody!,
            x => x.Key == k);

        var find = await client.GetAsync(
            new Uri($"/api/v1/memory/semantics/find?userId=1&key={Uri.EscapeDataString(k[..8])}&domain=work&take=8", UriKind.Relative));
        find.EnsureSuccessStatusCode();

        var arch = await client.PostAsync(
            new Uri($"/api/v1/memory/semantics/{sem.Id}/archive?userId=1", UriKind.Relative),
            null);
        arch.EnsureSuccessStatusCode();
    }
}
