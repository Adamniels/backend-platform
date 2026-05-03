using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Application.Features.Access.UnlockSession;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.SideLearning;
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
}
