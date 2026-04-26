using Platform.Application.Features.Memory.Embeddings;
using Platform.Domain.Features.Memory;
using Platform.Domain.Features.Memory.Entities;
using Xunit;

namespace Platform.UnitTests;

public sealed class DocumentMemoryChunkBuilderTests
{
    [Fact]
    public void Note_type_yields_single_chunk_index_zero()
    {
        var t = DateTimeOffset.UtcNow;
        var item = MemoryItem.CreateNew(
            1,
            MemoryItemType.Note,
            "N",
            new string('x', 5000),
            "test",
            0.7,
            0.5,
            t);
        var chunks = DocumentMemoryChunkBuilder.BuildChunks(item, new DocumentMemoryChunkingOptions());
        Assert.Single(chunks);
        Assert.Equal(0, chunks[0].ChunkIndex);
        Assert.Contains("N", chunks[0].CanonicalText, StringComparison.Ordinal);
    }

    [Fact]
    public void Short_document_uses_whole_item_canonical()
    {
        var t = DateTimeOffset.UtcNow;
        var item = MemoryItem.CreateNew(
            1,
            MemoryItemType.Document,
            "Doc",
            new string('a', 100),
            "test",
            0.6,
            0.5,
            t);
        var chunks = DocumentMemoryChunkBuilder.BuildChunks(
            item,
            new DocumentMemoryChunkingOptions { MaxCharsBeforeChunking = 1800 });
        Assert.Single(chunks);
        Assert.Equal(0, chunks[0].ChunkIndex);
        Assert.Equal(MemoryEmbeddingCanonicalText.ForTitleAndContent("Doc", item.Content), chunks[0].CanonicalText);
    }

    [Fact]
    public void Long_document_splits_into_multiple_chunks()
    {
        var t = DateTimeOffset.UtcNow;
        var body = new string('b', 4000);
        var item = MemoryItem.CreateNew(
            1,
            MemoryItemType.Document,
            "Title",
            body,
            "test",
            0.6,
            0.5,
            t);
        var opts = new DocumentMemoryChunkingOptions
        {
            MaxCharsBeforeChunking = 500,
            MaxChunkBodyChars = 1000,
            OverlapChars = 100,
        };
        var chunks = DocumentMemoryChunkBuilder.BuildChunks(item, opts);
        Assert.True(chunks.Count >= 2);
        Assert.Equal(0, chunks[0].ChunkIndex);
        Assert.Equal(chunks.Count - 1, chunks[^1].ChunkIndex);
        foreach (var c in chunks)
        {
            Assert.Contains("chunk:", c.CanonicalText, StringComparison.Ordinal);
        }
    }
}
