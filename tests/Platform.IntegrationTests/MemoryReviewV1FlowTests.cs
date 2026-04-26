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
}
