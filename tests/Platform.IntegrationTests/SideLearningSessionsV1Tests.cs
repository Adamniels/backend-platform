using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Features.Access.UnlockSession;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.SideLearning;
using Platform.IntegrationTests.Infrastructure;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class SideLearningSessionsV1Tests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private const string ServiceToken = "integration-memory-worker-token";
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Internal_side_learning_rejects_wrong_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");
        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/side-learning/sessions/sl-test/proposals", UriKind.Relative),
            new PostSideLearningTopicProposalsV1Request
            {
                Topics =
                [
                    new SideLearningTopicProposalV1Item
                    {
                        Title = "Topic",
                        Rationale = "R",
                        EstimatedMinutes = 30,
                        Difficulty = "medium",
                        TargetSkillGap = "gap",
                    },
                ],
            });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Create_session_then_internal_proposals_advances_phase()
    {
        using var userClient = factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await userClient.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var createRes = await userClient.PostAsJsonAsync(
            new Uri("/api/v1/side-learning/sessions", UriKind.Relative),
            new CreateSideLearningSessionV1Request { InitialPrompt = "Learn Temporal" });
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateSideLearningSessionV1Response>(JsonReadOptions);
        Assert.NotNull(created);
        var sessionId = created!.SessionId;

        using var internalClient = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        internalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);
        var post = await internalClient.PostAsJsonAsync(
            new Uri($"/api/internal/v1/side-learning/sessions/{sessionId}/proposals", UriKind.Relative),
            new PostSideLearningTopicProposalsV1Request
            {
                Topics =
                [
                    new SideLearningTopicProposalV1Item
                    {
                        Title = "Temporal basics",
                        Rationale = "You asked about Temporal",
                        EstimatedMinutes = 45,
                        Difficulty = "intermediate",
                        TargetSkillGap = "workflows",
                    },
                ],
            });
        post.EnsureSuccessStatusCode();

        var getRes = await userClient.GetAsync(new Uri($"/api/v1/side-learning/sessions/{sessionId}", UriKind.Relative));
        getRes.EnsureSuccessStatusCode();
        var state = await getRes.Content.ReadFromJsonAsync<SideLearningSessionV1Dto>(JsonReadOptions);
        Assert.NotNull(state);
        Assert.Equal("awaitingTopicSelection", state!.Phase);
        Assert.Contains("Temporal basics", state.TopicProposalsJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Internal_session_content_when_generating_advances_to_session_ready()
    {
        await using var scopedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                foreach (var d in services.Where(d => d.ServiceType == typeof(IWorkflowStarter)).ToList())
                {
                    services.Remove(d);
                }

                services.AddSingleton<IWorkflowStarter, StubTemporalWorkflowStarter>();
            });
        });

        using var userClient = scopedFactory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await userClient.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var createRes = await userClient.PostAsJsonAsync(
            new Uri("/api/v1/side-learning/sessions", UriKind.Relative),
            new CreateSideLearningSessionV1Request { InitialPrompt = "Learn Rust" });
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateSideLearningSessionV1Response>(JsonReadOptions);
        Assert.NotNull(created);
        var sessionId = created!.SessionId;

        using var internalClient = scopedFactory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        internalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);
        var postTopics = await internalClient.PostAsJsonAsync(
            new Uri($"/api/internal/v1/side-learning/sessions/{sessionId}/proposals", UriKind.Relative),
            new PostSideLearningTopicProposalsV1Request
            {
                Topics =
                [
                    new SideLearningTopicProposalV1Item
                    {
                        Title = "Rust ownership",
                        Rationale = "Foundations",
                        EstimatedMinutes = 40,
                        Difficulty = "intermediate",
                        TargetSkillGap = "memory",
                    },
                ],
            });
        postTopics.EnsureSuccessStatusCode();

        var selectRes = await userClient.PostAsJsonAsync(
            new Uri($"/api/v1/side-learning/sessions/{sessionId}/select-topic", UriKind.Relative),
            new SelectSideLearningTopicV1Request { TopicTitle = "Rust ownership", Feedback = "more exercises" });
        selectRes.EnsureSuccessStatusCode();

        var generating = await userClient.GetAsync(new Uri($"/api/v1/side-learning/sessions/{sessionId}", UriKind.Relative));
        generating.EnsureSuccessStatusCode();
        var genState = await generating.Content.ReadFromJsonAsync<SideLearningSessionV1Dto>(JsonReadOptions);
        Assert.NotNull(genState);
        Assert.Equal("generatingSession", genState!.Phase);

        const string sessionContentPayload = /* lang=json */ """
{
  "sections": [
    {"id":"goal","label":"Goal","estimatedMinutes":5,"type":"goal","content":"c1","example":"e1"},
    {"id":"context","label":"Context","estimatedMinutes":10,"type":"context","content":"c2","youtubeQuery":"rust ownership"},
    {"id":"hands-on","label":"Hands-on","estimatedMinutes":30,"type":"hands-on","content":"c3","outputType":"code"},
    {"id":"reflection","label":"Reflection","estimatedMinutes":10,"type":"reflection","content":"c4","prompts":["a","b","c"]}
  ],
  "memoryProposals": []
}
""";
        using var doc = JsonDocument.Parse(sessionContentPayload);
        var root = doc.RootElement;
        var sessionBody = new PostSideLearningSessionContentV1Request
        {
            Sections = root.GetProperty("sections"),
            MemoryProposals = root.GetProperty("memoryProposals"),
        };

        var contentRes = await internalClient.PostAsJsonAsync(
            new Uri($"/api/internal/v1/side-learning/sessions/{sessionId}/session-content", UriKind.Relative),
            sessionBody);
        contentRes.EnsureSuccessStatusCode();

        var final = await userClient.GetAsync(new Uri($"/api/v1/side-learning/sessions/{sessionId}", UriKind.Relative));
        final.EnsureSuccessStatusCode();
        var finalState = await final.Content.ReadFromJsonAsync<SideLearningSessionV1Dto>(JsonReadOptions);
        Assert.NotNull(finalState);
        Assert.Equal("sessionReady", finalState!.Phase);
        Assert.Contains("goal", finalState.SessionContentJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Reflection_insights_when_analyzing_advances_to_completed()
    {
        await using var scopedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                foreach (var d in services.Where(d => d.ServiceType == typeof(IWorkflowStarter)).ToList())
                {
                    services.Remove(d);
                }

                services.AddSingleton<IWorkflowStarter, StubTemporalWorkflowStarter>();
            });
        });

        using var userClient = scopedFactory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await userClient.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var createRes = await userClient.PostAsJsonAsync(
            new Uri("/api/v1/side-learning/sessions", UriKind.Relative),
            new CreateSideLearningSessionV1Request { InitialPrompt = "Learn Go" });
        createRes.EnsureSuccessStatusCode();
        var created = await createRes.Content.ReadFromJsonAsync<CreateSideLearningSessionV1Response>(JsonReadOptions);
        Assert.NotNull(created);
        var sessionId = created!.SessionId;

        using var internalClient = scopedFactory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        internalClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);
        (await internalClient.PostAsJsonAsync(
            new Uri($"/api/internal/v1/side-learning/sessions/{sessionId}/proposals", UriKind.Relative),
            new PostSideLearningTopicProposalsV1Request
            {
                Topics =
                [
                    new SideLearningTopicProposalV1Item
                    {
                        Title = "Go modules",
                        Rationale = "R",
                        EstimatedMinutes = 30,
                        Difficulty = "intermediate",
                        TargetSkillGap = "modules",
                    },
                ],
            })).EnsureSuccessStatusCode();

        (await userClient.PostAsJsonAsync(
            new Uri($"/api/v1/side-learning/sessions/{sessionId}/select-topic", UriKind.Relative),
            new SelectSideLearningTopicV1Request { TopicTitle = "Go modules", Feedback = null })).EnsureSuccessStatusCode();

        const string sessionContentPayload = /* lang=json */ """
{
  "sections": [
    {"id":"goal","label":"Goal","estimatedMinutes":5,"type":"goal","content":"c1","example":"e1"},
    {"id":"context","label":"Context","estimatedMinutes":10,"type":"context","content":"c2","youtubeQuery":"go modules"},
    {"id":"hands-on","label":"Hands-on","estimatedMinutes":30,"type":"hands-on","content":"c3","outputType":"code"},
    {"id":"reflection","label":"Reflection","estimatedMinutes":10,"type":"reflection","content":"c4","prompts":["a","b","c"]}
  ],
  "memoryProposals": []
}
""";
        using var sessionDoc = JsonDocument.Parse(sessionContentPayload);
        var sessionRoot = sessionDoc.RootElement;
        (await internalClient.PostAsJsonAsync(
            new Uri($"/api/internal/v1/side-learning/sessions/{sessionId}/session-content", UriKind.Relative),
            new PostSideLearningSessionContentV1Request
            {
                Sections = sessionRoot.GetProperty("sections"),
                MemoryProposals = sessionRoot.GetProperty("memoryProposals"),
            })).EnsureSuccessStatusCode();

        foreach (var sid in new[] { "goal", "context", "hands-on", "reflection" })
        {
            (await userClient.PostAsJsonAsync(
                new Uri($"/api/v1/side-learning/sessions/{sessionId}/progress", UriKind.Relative),
                new UpdateSideLearningProgressV1Request { SectionId = sid, Completed = true })).EnsureSuccessStatusCode();
        }

        var beforeReflect = await userClient.GetAsync(new Uri($"/api/v1/side-learning/sessions/{sessionId}", UriKind.Relative));
        beforeReflect.EnsureSuccessStatusCode();
        var br = await beforeReflect.Content.ReadFromJsonAsync<SideLearningSessionV1Dto>(JsonReadOptions);
        Assert.NotNull(br);
        Assert.Equal("awaitingReflection", br!.Phase);

        (await userClient.PostAsJsonAsync(
            new Uri($"/api/v1/side-learning/sessions/{sessionId}/reflect", UriKind.Relative),
            new SubmitSideLearningReflectionV1Request
            {
                Reflection = "The hands-on section was useful; I want more short exercises next time.",
            })).EnsureSuccessStatusCode();

        var analyzing = await userClient.GetAsync(new Uri($"/api/v1/side-learning/sessions/{sessionId}", UriKind.Relative));
        analyzing.EnsureSuccessStatusCode();
        var ar = await analyzing.Content.ReadFromJsonAsync<SideLearningSessionV1Dto>(JsonReadOptions);
        Assert.NotNull(ar);
        Assert.Equal("analyzingReflection", ar!.Phase);

        (await internalClient.PostAsJsonAsync(
            new Uri($"/api/internal/v1/side-learning/sessions/{sessionId}/reflection-insights", UriKind.Relative),
            new PostSideLearningReflectionInsightsV1Request { MemoryProposals = [] })).EnsureSuccessStatusCode();

        var done = await userClient.GetAsync(new Uri($"/api/v1/side-learning/sessions/{sessionId}", UriKind.Relative));
        done.EnsureSuccessStatusCode();
        var doneState = await done.Content.ReadFromJsonAsync<SideLearningSessionV1Dto>(JsonReadOptions);
        Assert.NotNull(doneState);
        Assert.Equal("completed", doneState!.Phase);
    }
}
