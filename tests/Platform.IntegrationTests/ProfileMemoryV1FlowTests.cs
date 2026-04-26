using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Xunit;

namespace Platform.IntegrationTests;

public sealed class ProfileMemoryV1FlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Get_explicit_profile_after_unlock_is_ok_and_put_round_trips()
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

        var get0 = await client.GetAsync(new Uri("/api/v1/memory/explicit-profile", UriKind.Relative));
        get0.EnsureSuccessStatusCode();
        var empty = await get0.Content.ReadFromJsonAsync<ProfileMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(empty);
        Assert.Equal(1.0, empty.AuthorityWeight);

        var put = new UpdateProfileMemoryV1Request
        {
            CoreInterests = new[] { "learning", "safety" },
            SecondaryInterests = new[] { "music" },
            Goals = new[] { "ship features" },
            Preferences =
            [
                new ProfileMemoryPreferenceV1 { Key = "theme", Value = "dark" },
            ],
            ActiveProjects = [new ProfileMemoryProjectV1 { Name = "Memory", ExternalId = "m1" }],
            SkillLevels = [new ProfileMemorySkillLevelV1 { Name = "dotnet", Level = 0.8 }],
        };

        var putRes = await client.PutAsJsonAsync(
            new Uri("/api/v1/memory/explicit-profile", UriKind.Relative),
            put);
        putRes.EnsureSuccessStatusCode();
        var afterPut = await putRes.Content.ReadFromJsonAsync<ProfileMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(afterPut);
        Assert.NotNull(afterPut.Id);
        Assert.Equal(1.0, afterPut.AuthorityWeight);
        Assert.Equal("learning", afterPut.CoreInterests[0]);
        Assert.Equal("m1", afterPut.ActiveProjects[0].ExternalId);
        Assert.Equal(0.8, afterPut.SkillLevels[0].Level);

        var get1 = await client.GetAsync(new Uri("/api/v1/memory/explicit-profile", UriKind.Relative));
        get1.EnsureSuccessStatusCode();
        var roundTrip = await get1.Content.ReadFromJsonAsync<ProfileMemoryV1Dto>(JsonReadOptions);
        Assert.NotNull(roundTrip);
        Assert.Equal(afterPut.Id, roundTrip.Id);
        Assert.Equal("ship features", roundTrip.Goals[0]);
    }
}
