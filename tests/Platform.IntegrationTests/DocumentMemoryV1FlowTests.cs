using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Features.Memory.Embeddings;
using Platform.Contracts.Admin;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;
using Platform.Infrastructure.Persistence;
using Xunit;

namespace Platform.IntegrationTests;

[Collection("integration memory")]
public sealed class DocumentMemoryV1FlowTests(PlatformWebApplicationFactory factory) : IClassFixture<PlatformWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonReadOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Post_document_indexes_chunks_and_context_returns_document_evidence()
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

        var body = new string('z', 2500);
        var ingestRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/documents", UriKind.Relative),
            new IngestDocumentMemoryV1Request
            {
                UserId = MemoryUser.DefaultId,
                Title = "Architecture notes",
                Content = body,
                SourceType = "architecture-doc",
                ProjectId = "proj-doc-test",
                Domain = "workflow",
                IndexEmbeddings = true,
            });
        ingestRes.EnsureSuccessStatusCode();
        var ingest = await ingestRes.Content.ReadFromJsonAsync<IngestDocumentMemoryV1Response>(JsonReadOptions);
        Assert.NotNull(ingest);
        Assert.True(ingest!.EmbeddingChunksWritten >= 2, "long document should produce multiple embedding chunks");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
            var n = db.MemoryEmbeddings.Count(
                x => x.MemoryItemId == ingest.MemoryItemId);
            Assert.Equal(ingest.EmbeddingChunksWritten, n);
        }

        var firstPiece = body.Substring(0, DocumentMemoryChunkingOptions.DefaultMaxChunkBodyChars);
        var firstChunkCanonical = MemoryEmbeddingCanonicalText.ForDocumentChunk("Architecture notes", 0, firstPiece);
        var ctxRes = await client.PostAsJsonAsync(
            new Uri("/api/v1/memory/context", UriKind.Relative),
            new GetMemoryContextV1Request
            {
                UserId = MemoryUser.DefaultId,
                TaskDescription = firstChunkCanonical,
                IncludeVectorRecall = true,
                ProjectId = "proj-doc-test",
                Domain = "workflow",
            });
        ctxRes.EnsureSuccessStatusCode();
        var ctx = await ctxRes.Content.ReadFromJsonAsync<MemoryContextV1Dto>(JsonReadOptions);
        Assert.NotNull(ctx);
        var hit = ctx!.MemoryItemVectorRecalls.FirstOrDefault(x => x.MemoryItemId == ingest.MemoryItemId);
        Assert.NotNull(hit);
        Assert.True(hit!.IsDocumentEvidence);
        Assert.Equal("proj-doc-test", hit.ProjectId);
        Assert.Equal("workflow", hit.Domain);
        Assert.Equal("architecture-doc", hit.SourceType);
        Assert.True(hit.ChunkIndex >= 0);
    }
}
