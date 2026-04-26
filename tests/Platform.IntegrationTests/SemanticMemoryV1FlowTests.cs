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

    [Fact]
    public async Task Memory_context_includes_semantic_evidence_provenance()
    {
        var k = $"sem-prov-{Guid.NewGuid():N}";
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
                EventType = "integration.provenance",
                UserId = 1,
                OccurredAt = DateTimeOffset.UtcNow,
                PayloadJson = "{\"p\":1}",
            });
        evRes.EnsureSuccessStatusCode();
        var ev = await evRes.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);
        Assert.NotNull(ev);

        var create = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/semantics", UriKind.Relative),
            new CreateSemanticMemoryV1Request
            {
                UserId = 1,
                Key = k,
                Claim = "provenance visible in context",
                Confidence = 0.66d,
                Domain = "work",
                Status = "Active",
                EventId = ev!.Id,
                EvidenceStrength = 0.8d,
                EvidenceReason = "integration",
            });
        create.EnsureSuccessStatusCode();
        var sem = await create.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(sem);

        var ctx = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/context", UriKind.Relative),
            new GetMemoryContextV1Request
            {
                TaskDescription = $"explain {k} and preferences",
                IncludeVectorRecall = false,
            });
        ctx.EnsureSuccessStatusCode();
        var body = await ctx.Content.ReadFromJsonAsync<MemoryContextV1Dto>(JsonReadOptions);
        Assert.NotNull(body);
        var row = body!.SemanticMemories.FirstOrDefault(s => s.Key == k);
        Assert.NotNull(row);
        Assert.True(row!.EvidenceLinkCount >= 1);
        Assert.Contains(ev.Id, row.SupportingEventIds);
    }

    [Fact]
    public async Task Attach_same_evidence_twice_is_idempotent()
    {
        var k = $"sem-idem-{Guid.NewGuid():N}";
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            });

        await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var ev1Res = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/events", UriKind.Relative),
            new IngestMemoryEventV1Request
            {
                EventType = "integration.idem.seed",
                UserId = 1,
                OccurredAt = DateTimeOffset.UtcNow,
            });
        ev1Res.EnsureSuccessStatusCode();
        var ev1 = await ev1Res.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);
        Assert.NotNull(ev1);

        var create = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/semantics", UriKind.Relative),
            new CreateSemanticMemoryV1Request
            {
                UserId = 1,
                Key = k,
                Claim = "idem attach",
                Confidence = 0.5d,
                Domain = "work",
                Status = "Active",
                EventId = ev1!.Id,
                EvidenceStrength = 0.5d,
            });
        create.EnsureSuccessStatusCode();
        var sem = await create.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(sem);
        var c0 = sem!.Confidence;

        var ev2Res = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/events", UriKind.Relative),
            new IngestMemoryEventV1Request
            {
                EventType = "integration.idem.extra",
                UserId = 1,
                OccurredAt = DateTimeOffset.UtcNow,
            });
        ev2Res.EnsureSuccessStatusCode();
        var ev2 = await ev2Res.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);
        Assert.NotNull(ev2);

        var attachBody = new AttachSemanticMemoryEvidenceV1Request
        {
            UserId = 1,
            EventId = ev2!.Id,
            Strength = 0.4d,
            FromInferredSource = false,
            ReinforceConfidence = true,
            ReinforceConfidenceDelta = 0.05d,
        };

        var a1 = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/semantics/{sem.Id}/evidence", UriKind.Relative),
            attachBody);
        a1.EnsureSuccessStatusCode();
        var s1 = await a1.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(s1);
        Assert.True(s1!.Confidence > c0);

        var a2 = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/semantics/{sem.Id}/evidence", UriKind.Relative),
            attachBody);
        a2.EnsureSuccessStatusCode();
        var s2 = await a2.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(s2);
        Assert.Equal(s1.Confidence, s2!.Confidence);
    }
}
