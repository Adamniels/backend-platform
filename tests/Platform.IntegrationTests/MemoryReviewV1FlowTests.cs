using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class MemoryReviewV1FlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    private static string NewSemanticJson(string key, string claim, double conf = 0.65d) =>
        JsonSerializer.Serialize(
            new { kind = "NewSemantic", key, claim, initialConfidence = conf },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static string ReviseSemanticJson(long semanticId, string newClaim, double? newConfidence = null) =>
        JsonSerializer.Serialize(
            new { kind = "ReviseSemanticClaim", semanticMemoryId = semanticId, newClaim, newConfidence },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static string SupersedeSemanticJson(long supersededSemanticId, long canonicalSemanticId, string reason) =>
        JsonSerializer.Serialize(
            new { kind = "SupersedeSemantic", supersededSemanticId, canonicalSemanticId, reason },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    private static string ConflictWithExplicitProfileJson(long semanticId, string key, string claim, string explicitText) =>
        JsonSerializer.Serialize(
            new
            {
                kind = "ConflictWithExplicitProfile",
                semanticMemoryId = semanticId,
                key,
                claim,
                explicitKind = "core_interest",
                explicitText,
            },
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

    [Fact]
    public async Task Create_patch_approve_creates_semantic_and_clears_pending_list()
    {
        var key = $"sem-{Guid.NewGuid():N}";
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            });

        await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var createRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/review-queue", UriKind.Relative),
            new CreateMemoryReviewQueueItemV1Request
            {
                ProposalType = "NewSemantic",
                Title = "Proposed claim",
                Summary = "Please confirm",
                ProposedChangeJson = NewSemanticJson(key, "original claim", 0.55d),
                Priority = 5,
            });
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<MemoryReviewQueueItemV1Dto>(JsonReadOptions);
        Assert.NotNull(created);
        var id = created!.Id;

        var patchReq = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/memory/review-queue/{id}")
        {
            Content = JsonContent.Create(
                new PatchMemoryReviewQueueItemV1Request
                {
                    ProposedChangeJson = NewSemanticJson(key, "revised claim after edit", 0.7d),
                },
                options: new JsonSerializerOptions(JsonSerializerDefaults.Web)),
        };
        var patchRes = await client.SendAsync(patchReq);
        patchRes.EnsureSuccessStatusCode();

        var approveRes = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/review-queue/{id}/approve", UriKind.Relative),
            new ApproveMemoryReviewQueueItemV1Request { ReviewNotes = "lgtm" });
        approveRes.EnsureSuccessStatusCode();
        var approveBody = await approveRes.Content.ReadFromJsonAsync<ApproveMemoryReviewQueueItemV1Response>(JsonReadOptions);
        Assert.NotNull(approveBody?.SemanticMemoryId);
        Assert.True(approveBody.SemanticMemoryId > 0);

        var list = await client.GetAsync(new Uri("/api/v1/memory/review-queue", UriKind.Relative));
        list.EnsureSuccessStatusCode();
        var pending = await list.Content.ReadFromJsonAsync<List<MemoryReviewQueueItemV1Dto>>(JsonReadOptions);
        Assert.NotNull(pending);
        Assert.DoesNotContain(pending!, x => x.Id == id);
    }

    [Fact]
    public async Task Reject_leaves_item_resolved_and_out_of_pending()
    {
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            });

        await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var createRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/review-queue", UriKind.Relative),
            new CreateMemoryReviewQueueItemV1Request
            {
                ProposalType = "NewSemantic",
                Title = "Reject me",
                Summary = "s",
                ProposedChangeJson = NewSemanticJson($"rej-{Guid.NewGuid():N}", "c"),
            });
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<MemoryReviewQueueItemV1Dto>(JsonReadOptions);
        Assert.NotNull(created);
        var id = created!.Id;

        var rejectRes = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/review-queue/{id}/reject", UriKind.Relative),
            new RejectMemoryReviewQueueItemV1Request { Reason = "not accurate" });
        Assert.Equal(HttpStatusCode.NoContent, rejectRes.StatusCode);

        var list = await client.GetAsync(new Uri("/api/v1/memory/review-queue", UriKind.Relative));
        list.EnsureSuccessStatusCode();
        var pending = await list.Content.ReadFromJsonAsync<List<MemoryReviewQueueItemV1Dto>>(JsonReadOptions);
        Assert.NotNull(pending);
        Assert.DoesNotContain(pending!, x => x.Id == id);
    }

    [Fact]
    public async Task Approve_revise_semantic_claim_updates_existing_semantic()
    {
        var key = $"rev-sem-{Guid.NewGuid():N}";
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await client.PostAsJsonAsync(new Uri("/api/admin/unlock", UriKind.Relative), new UnlockRequest("integration-test-access-key"));

        var evRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/events", UriKind.Relative),
            new IngestMemoryEventV1Request { EventType = "integration.revise.semantic", UserId = 1, OccurredAt = DateTimeOffset.UtcNow });
        evRes.EnsureSuccessStatusCode();
        var ev = await evRes.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);
        Assert.NotNull(ev);

        var createSemRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/semantics", UriKind.Relative),
            new CreateSemanticMemoryV1Request
            {
                UserId = 1,
                Key = key,
                Claim = "old claim",
                Confidence = 0.55d,
                EventId = ev!.Id,
                EvidenceStrength = 0.65d,
            });
        createSemRes.EnsureSuccessStatusCode();
        var semantic = await createSemRes.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(semantic);

        var createReview = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/review-queue", UriKind.Relative),
            new CreateMemoryReviewQueueItemV1Request
            {
                ProposalType = "ReviseSemanticClaim",
                Title = "Revise semantic claim",
                Summary = "Update claim wording",
                ProposedChangeJson = ReviseSemanticJson(semantic!.Id, "new approved claim", 0.91d),
            });
        createReview.EnsureSuccessStatusCode();
        var review = await createReview.Content.ReadFromJsonAsync<MemoryReviewQueueItemV1Dto>(JsonReadOptions);
        Assert.NotNull(review);

        var approve = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/review-queue/{review!.Id}/approve", UriKind.Relative),
            new ApproveMemoryReviewQueueItemV1Request());
        approve.EnsureSuccessStatusCode();
        var approveBody = await approve.Content.ReadFromJsonAsync<ApproveMemoryReviewQueueItemV1Response>(JsonReadOptions);
        Assert.Equal(semantic.Id, approveBody?.SemanticMemoryId);

        var get = await client.GetAsync(new Uri($"/api/v1/memory/semantics/{semantic.Id}?userId=1", UriKind.Relative));
        get.EnsureSuccessStatusCode();
        var updated = await get.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(updated);
        Assert.Equal("new approved claim", updated!.Claim);
    }

    [Fact]
    public async Task Approve_supersede_semantic_marks_old_row_superseded()
    {
        var key = $"sup-sem-{Guid.NewGuid():N}";
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await client.PostAsJsonAsync(new Uri("/api/admin/unlock", UriKind.Relative), new UnlockRequest("integration-test-access-key"));

        async Task<SemanticMemoryV1Dto> CreateSemanticAsync(string claim, string eventType, string domain)
        {
            var evRes = await client.PostAsJsonAsync(
                new Uri("/api/v1/memory/events", UriKind.Relative),
                new IngestMemoryEventV1Request { EventType = eventType, UserId = 1, OccurredAt = DateTimeOffset.UtcNow });
            evRes.EnsureSuccessStatusCode();
            var ev = await evRes.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);
            var semRes = await client.PostAsJsonAsync(
                new Uri("/api/v1/memory/semantics", UriKind.Relative),
                new CreateSemanticMemoryV1Request
                {
                    UserId = 1,
                    Key = key,
                    Claim = claim,
                    Confidence = 0.6d,
                    Domain = domain,
                    Status = "Pending",
                    EventId = ev!.Id,
                    EvidenceStrength = 0.7d,
                });
            semRes.EnsureSuccessStatusCode();
            var sem = await semRes.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
            Assert.NotNull(sem);
            return sem!;
        }

        var canonical = await CreateSemanticAsync("canonical claim", "integration.supersede.canonical", "work-a");
        var superseded = await CreateSemanticAsync("old claim", "integration.supersede.old", "work-b");

        var createReview = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/review-queue", UriKind.Relative),
            new CreateMemoryReviewQueueItemV1Request
            {
                ProposalType = "SupersedeSemantic",
                Title = "Supersede semantic",
                Summary = "Resolve overlap",
                ProposedChangeJson = SupersedeSemanticJson(superseded.Id, canonical.Id, "canonical chosen"),
            });
        createReview.EnsureSuccessStatusCode();
        var review = await createReview.Content.ReadFromJsonAsync<MemoryReviewQueueItemV1Dto>(JsonReadOptions);
        Assert.NotNull(review);

        var approve = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/review-queue/{review!.Id}/approve", UriKind.Relative),
            new ApproveMemoryReviewQueueItemV1Request());
        approve.EnsureSuccessStatusCode();

        var supersededGet = await client.GetAsync(new Uri($"/api/v1/memory/semantics/{superseded.Id}?userId=1", UriKind.Relative));
        supersededGet.EnsureSuccessStatusCode();
        var supersededRow = await supersededGet.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(supersededRow);
        Assert.Equal("Superseded", supersededRow!.Status);
    }

    [Fact]
    public async Task Approve_conflict_with_explicit_profile_proposal_is_supported()
    {
        var key = $"conf-sem-{Guid.NewGuid():N}";
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await client.PostAsJsonAsync(new Uri("/api/admin/unlock", UriKind.Relative), new UnlockRequest("integration-test-access-key"));

        var evRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/events", UriKind.Relative),
            new IngestMemoryEventV1Request { EventType = "integration.conflict.profile", UserId = 1, OccurredAt = DateTimeOffset.UtcNow });
        evRes.EnsureSuccessStatusCode();
        var ev = await evRes.Content.ReadFromJsonAsync<MemoryEventCreatedV1Dto>(JsonReadOptions);

        var semRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/semantics", UriKind.Relative),
            new CreateSemanticMemoryV1Request
            {
                UserId = 1,
                Key = key,
                Claim = "user is not interested in backend architecture",
                Confidence = 0.62d,
                EventId = ev!.Id,
                EvidenceStrength = 0.7d,
            });
        semRes.EnsureSuccessStatusCode();
        var semantic = await semRes.Content.ReadFromJsonAsync<SemanticMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(semantic);

        var reviewRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/review-queue", UriKind.Relative),
            new CreateMemoryReviewQueueItemV1Request
            {
                ProposalType = "ConflictWithExplicitProfile",
                Title = "Profile conflict",
                Summary = "Check contradiction",
                ProposedChangeJson = ConflictWithExplicitProfileJson(
                    semantic!.Id,
                    semantic.Key,
                    semantic.Claim,
                    "backend architecture"),
            });
        reviewRes.EnsureSuccessStatusCode();
        var review = await reviewRes.Content.ReadFromJsonAsync<MemoryReviewQueueItemV1Dto>(JsonReadOptions);
        Assert.NotNull(review);

        var approve = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/review-queue/{review!.Id}/approve", UriKind.Relative),
            new ApproveMemoryReviewQueueItemV1Request());
        approve.EnsureSuccessStatusCode();
        var body = await approve.Content.ReadFromJsonAsync<ApproveMemoryReviewQueueItemV1Response>(JsonReadOptions);
        Assert.Equal(semantic.Id, body?.SemanticMemoryId);
    }
}
