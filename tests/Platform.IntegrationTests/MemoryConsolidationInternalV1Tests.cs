using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Application.Features.Memory.Consolidation;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class MemoryConsolidationInternalV1Tests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private const string ServiceToken = "integration-memory-worker-token";
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Internal_consolidation_rejects_wrong_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");
        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/memory/consolidation/nightly", UriKind.Relative),
            new ExecuteNightlyMemoryConsolidationV1Request());
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Internal_consolidation_idempotent_and_creates_proposal_for_repeated_events()
    {
        var windowEnd = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var occurred = new DateTimeOffset(windowEnd.AddDays(-1).ToDateTime(new TimeOnly(14, 30), DateTimeKind.Utc));
        var idem = $"integ-{Guid.NewGuid():N}";
        var eventType = $"integration.repeat.{Guid.NewGuid():N}";

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);

        await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        for (var i = 0; i < 3; i++)
        {
            var ev = await client.PostAsJsonAsync(
                new Uri("/api/v1/memory/events", UriKind.Relative),
                new IngestMemoryEventV1Request
                {
                    EventType = eventType,
                    UserId = 1,
                    OccurredAt = occurred.AddMinutes(i),
                    PayloadJson = "{\"i\":" + i + "}",
                });
            ev.EnsureSuccessStatusCode();
        }

        var body = new ExecuteNightlyMemoryConsolidationV1Request
        {
            UserId = 1,
            WindowEndExclusiveUtc = windowEnd,
            IdempotencyKey = idem,
        };

        var first = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/memory/consolidation/nightly", UriKind.Relative),
            body);
        first.EnsureSuccessStatusCode();
        var r1 = await first.Content.ReadFromJsonAsync<NightlyMemoryConsolidationV1Response>(JsonReadOptions);
        Assert.NotNull(r1);
        Assert.False(r1!.FromCache);
        Assert.True(
            r1.ProcessedEventsCount >= 3,
            "Window should include at least the three ingested events (shared DB may contain more).");
        Assert.True(r1.ProposalsCreatedCount >= 1, "Expected at least one review proposal for repeated event type.");

        var second = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/memory/consolidation/nightly", UriKind.Relative),
            body);
        second.EnsureSuccessStatusCode();
        var r2 = await second.Content.ReadFromJsonAsync<NightlyMemoryConsolidationV1Response>(JsonReadOptions);
        Assert.NotNull(r2);
        Assert.True(r2!.FromCache);
        Assert.Equal(r1.RunId, r2.RunId);
    }

    [Fact]
    public async Task Consolidation_does_not_auto_reinforce_profile_prefixed_event_types()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var eventType = $"profile.consolidation.block.{suffix}";
        var semanticKey = MemoryConsolidationKeys.SemanticKeyFromEventType(eventType);
        var windowEnd = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var occurredBase = new DateTimeOffset(
            windowEnd.AddDays(-1).ToDateTime(new TimeOnly(11, 15), DateTimeKind.Utc));
        var idem = $"integ-block-{suffix}";

        using var userClient = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await userClient.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var seedEv = await userClient.PostAsJsonAsync(
            new Uri("/api/v1/memory/events", UriKind.Relative),
            new IngestMemoryEventV1Request
            {
                EventType = eventType + ".seed",
                UserId = 1,
                OccurredAt = occurredBase,
            });
        seedEv.EnsureSuccessStatusCode();
        var seed = await seedEv.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);
        Assert.NotNull(seed);

        var createSem = await userClient.PostAsJsonAsync(
            new Uri("/api/v1/memory/semantics", UriKind.Relative),
            new CreateSemanticMemoryV1Request
            {
                UserId = 1,
                Key = semanticKey,
                Claim = "blocked auto reinforce test",
                Confidence = 0.52d,
                AuthorityWeight = 0.55d,
                Domain = null,
                Status = "Active",
                EventId = seed!.Id,
                EvidenceStrength = 0.5d,
            });
        createSem.EnsureSuccessStatusCode();
        var sem = await createSem.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(sem);
        var confidenceBefore = sem!.Confidence;

        for (var i = 0; i < 3; i++)
        {
            var ev = await userClient.PostAsJsonAsync(
                new Uri("/api/v1/memory/events", UriKind.Relative),
                new IngestMemoryEventV1Request
                {
                    EventType = eventType,
                    UserId = 1,
                    OccurredAt = occurredBase.AddMinutes(i + 1),
                });
            ev.EnsureSuccessStatusCode();
        }

        using var internalClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        internalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);
        var run = await internalClient.PostAsJsonAsync(
            new Uri("/api/internal/v1/memory/consolidation/nightly", UriKind.Relative),
            new ExecuteNightlyMemoryConsolidationV1Request
            {
                UserId = 1,
                WindowEndExclusiveUtc = windowEnd,
                IdempotencyKey = idem,
            });
        run.EnsureSuccessStatusCode();

        var list = await userClient.GetAsync(new Uri("/api/v1/memory/semantics?userId=1&includePending=true", UriKind.Relative));
        list.EnsureSuccessStatusCode();
        var listBody = await list.Content.ReadFromJsonAsync<SemanticMemoryV1Dto[]>(JsonReadOptions);
        Assert.NotNull(listBody);
        var after = listBody!.FirstOrDefault(x => x.Key == semanticKey);
        Assert.NotNull(after);
        Assert.Equal(confidenceBefore, after!.Confidence);
    }
}
