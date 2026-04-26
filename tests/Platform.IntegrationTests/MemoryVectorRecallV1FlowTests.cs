using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class MemoryVectorRecallV1FlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Upsert_embedding_then_context_returns_vector_recall_for_task_text()
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

        const string title = "Vector doc";
        const string content =
            "This document discusses cosine similarity and embeddings for retrieval.";
        long itemId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            var t = DateTimeOffset.UtcNow;
            var item = MemoryItem.CreateNew(
                MemoryUser.DefaultId,
                MemoryItemType.Document,
                title,
                content,
                "integration-test",
                0.75,
                0.6,
                t);
            item.PromoteToActive(t);
            db.MemoryItems.Add(item);
            await db.SaveChangesAsync();
            itemId = item.Id;
        }

        var taskAsCanonical = $"{title}\u001F{content}";

        var upsertRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/embeddings/upsert", UriKind.Relative),
            new UpsertMemoryEmbeddingV1Request
            {
                UserId = MemoryUser.DefaultId,
                MemoryItemId = itemId,
            });
        upsertRes.EnsureSuccessStatusCode();

        var ctxRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/context", UriKind.Relative),
            new GetMemoryContextV1Request
            {
                UserId = MemoryUser.DefaultId,
                TaskDescription = taskAsCanonical,
                IncludeVectorRecall = true,
            });
        ctxRes.EnsureSuccessStatusCode();
        var body = await ctxRes.Content.ReadFromJsonAsync<MemoryContextV1Dto>(JsonReadOptions);
        Assert.NotNull(body);
        Assert.True(body!.VectorRecallUsed);
        Assert.Equal("v1-sql+vector", body.AssemblyStage);
        var hit = body.MemoryItemVectorRecalls.FirstOrDefault(x => x.MemoryItemId == itemId);
        Assert.NotNull(hit);
        Assert.True(hit!.CosineSimilarity > 0.25d);
    }
}
