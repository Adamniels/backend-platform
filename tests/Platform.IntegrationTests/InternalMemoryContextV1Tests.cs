using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.Contracts.V1.Memory;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class InternalMemoryContextV1Tests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private const string ServiceToken = "integration-memory-worker-token";
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Internal_memory_context_rejects_wrong_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "wrong");
        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/memory/context", UriKind.Relative),
            new GetMemoryContextV1Request
            {
                TaskDescription = "test",
                WorkflowType = "side_learning",
                Domain = "learning",
                IncludeVectorRecall = false,
            });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Internal_memory_context_returns_payload_with_valid_bearer()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ServiceToken);
        var res = await client.PostAsJsonAsync(
            new Uri("/api/internal/v1/memory/context", UriKind.Relative),
            new GetMemoryContextV1Request
            {
                TaskDescription = "learn distributed systems",
                WorkflowType = "side_learning",
                Domain = "learning",
                IncludeVectorRecall = false,
            });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<MemoryContextV1Dto>(JsonReadOptions);
        Assert.NotNull(body);
        Assert.Equal("v1-sql", body!.AssemblyStage);
        Assert.NotNull(body.ProfileFacts);
        Assert.NotNull(body.SemanticMemories);
        Assert.NotNull(body.ProceduralRules);
        Assert.NotNull(body.MemoryItemVectorRecalls);
    }
}
