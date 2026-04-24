using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.Admin;
using Xunit;

namespace Platform.IntegrationTests;

public sealed class AccessFlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static HttpClient CreateClient(PlatformWebApplicationFactory f) =>
        f.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

    [Fact]
    public async Task Protected_route_without_session_returns_401()
    {
        using var client = CreateClient(factory);
        var res = await client.GetAsync(new Uri("/api/v1/dashboard/summary", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Health_without_session_returns_401_when_public_health_disabled()
    {
        using var client = CreateClient(factory);
        var res = await client.GetAsync(new Uri("/health", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Unlock_with_wrong_key_returns_401()
    {
        using var client = CreateClient(factory);
        var res = await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("wrong-key"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Unlock_then_dashboard_succeeds_lock_clears_session()
    {
        using var client = CreateClient(factory);
        var unlock = await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));
        unlock.EnsureSuccessStatusCode();

        var dash = await client.GetAsync(new Uri("/api/v1/dashboard/summary", UriKind.Relative));
        dash.EnsureSuccessStatusCode();

        var session = await client.GetAsync(new Uri("/api/admin/session", UriKind.Relative));
        session.EnsureSuccessStatusCode();

        var locked = await client.PostAsync(new Uri("/api/admin/lock", UriKind.Relative), null);
        locked.EnsureSuccessStatusCode();

        var after = await client.GetAsync(new Uri("/api/v1/dashboard/summary", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }

    [Fact]
    public async Task Lock_without_session_still_returns_204()
    {
        using var client = CreateClient(factory);
        var locked = await client.PostAsync(new Uri("/api/admin/lock", UriKind.Relative), null);
        Assert.Equal(HttpStatusCode.NoContent, locked.StatusCode);
    }

    [Fact]
    public async Task Stats_endpoint_returns_payload_after_unlock()
    {
        using var client = CreateClient(factory);
        await client.PostAsJsonAsync(
            new Uri("/api/admin/unlock", UriKind.Relative),
            new UnlockRequest("integration-test-access-key"));

        var res = await client.GetAsync(new Uri("/api/v1/stats", UriKind.Relative));
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadAsStringAsync();
        Assert.Contains("tiles", json, StringComparison.Ordinal);
    }
}
