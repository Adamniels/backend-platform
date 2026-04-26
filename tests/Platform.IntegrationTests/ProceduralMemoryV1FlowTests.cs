using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class ProceduralMemoryV1FlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Create_high_authority_then_context_includes_source_and_authority()
    {
        var wf = $"wf-{Guid.NewGuid():N}";
        var name = $"rule-{Guid.NewGuid():N}";
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
            new Uri("/api/v1/memory/procedural-rules", UriKind.Relative),
            new CreateProceduralRuleV1Request
            {
                WorkflowType = wf,
                RuleName = name,
                RuleContent = "Prefer bullet summaries.",
                Priority = 3,
                Source = "integration-test",
                AuthorityWeight = 0.92,
                ForceSubmitForReview = false,
            });
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateProceduralRuleV1Response>(JsonReadOptions);
        Assert.NotNull(created);
        Assert.Equal("Activated", created!.Outcome);
        Assert.NotNull(created.RuleId);

        var ctxRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/context", UriKind.Relative),
            new GetMemoryContextV1Request
            {
                TaskDescription = "summaries and learning",
                WorkflowType = wf,
            });
        ctxRes.EnsureSuccessStatusCode();
        var ctx = await ctxRes.Content.ReadFromJsonAsync<MemoryContextV1Dto>(JsonReadOptions);
        Assert.NotNull(ctx);
        var pr = ctx!.ProceduralRules.FirstOrDefault(r => r.RuleName == name);
        Assert.NotNull(pr);
        Assert.Equal("integration-test", pr!.Source);
        Assert.Equal(0.92d, pr.AuthorityWeight);
    }

    [Fact]
    public async Task Low_authority_create_queues_review_then_approve_activates_rule()
    {
        var wf = $"wf-{Guid.NewGuid():N}";
        var name = $"rule-{Guid.NewGuid():N}";
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
            new Uri("/api/v1/memory/procedural-rules", UriKind.Relative),
            new CreateProceduralRuleV1Request
            {
                WorkflowType = wf,
                RuleName = name,
                RuleContent = "Low trust content",
                Priority = 1,
                Source = "inferred:worker",
                AuthorityWeight = 0.55,
                ForceSubmitForReview = false,
            });
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateProceduralRuleV1Response>(JsonReadOptions);
        Assert.NotNull(created);
        Assert.Equal("PendingReview", created!.Outcome);
        Assert.NotNull(created.ReviewQueueItemId);

        var approveRes = await client.PostAsJsonAsync(
            new Uri($"/api/v1/memory/review-queue/{created.ReviewQueueItemId}/approve", UriKind.Relative),
            new ApproveMemoryReviewQueueItemV1Request());
        approveRes.EnsureSuccessStatusCode();
        var approveBody = await approveRes.Content.ReadFromJsonAsync<ApproveMemoryReviewQueueItemV1Response>(JsonReadOptions);
        Assert.NotNull(approveBody?.ProceduralRuleId);
        Assert.True(approveBody!.ProceduralRuleId > 0);

        var list = await client.GetAsync(new Uri("/api/v1/memory/procedural-rules", UriKind.Relative));
        list.EnsureSuccessStatusCode();
        var rows = await list.Content.ReadFromJsonAsync<List<ProceduralRuleSummaryV1Dto>>(JsonReadOptions);
        Assert.NotNull(rows);
        Assert.Contains(rows!, r => r.RuleName == name && r.Status == "Active");
    }
}
