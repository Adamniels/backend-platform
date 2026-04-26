using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class MemoryContextV1FlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Post_memory_context_prefers_explicit_goals_and_high_rank()
    {
        var unique = $"goal-{Guid.NewGuid():N}";
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true,
            });

        await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var putRes = await client.PutAsJsonAsync(
            new Uri("/api/v1/memory/explicit-profile", UriKind.Relative),
            new UpdateProfileMemoryV1Request
            {
                CoreInterests = new[] { "integration" },
                SecondaryInterests = new[] { "testing" },
                Goals = new[] { unique, "other" },
                Preferences = Array.Empty<ProfileMemoryPreferenceV1>(),
                ActiveProjects = Array.Empty<ProfileMemoryProjectV1>(),
                SkillLevels = Array.Empty<ProfileMemorySkillLevelV1>(),
            });
        putRes.EnsureSuccessStatusCode();
        var afterPut = await putRes.Content.ReadFromJsonAsync<ProfileMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(afterPut);
        Assert.Contains(unique, afterPut!.Goals);

        var res = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/context", UriKind.Relative),
            new GetMemoryContextV1Request
            {
                TaskDescription = $"plan work on {unique} and temporal",
            });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<MemoryContextV1Dto>(JsonReadOptions);
        Assert.NotNull(body);
        Assert.Equal("v1-sql", body.AssemblyStage);
        var goal = body.ActiveGoals.FirstOrDefault(g => g.Goal == unique);
        Assert.NotNull(goal);
        Assert.True(
            goal.RankScore >= 0.75d,
            "Explicit goals should achieve high rank scores (floor-boosted in v1).");
    }
}
