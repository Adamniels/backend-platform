using Microsoft.AspNetCore.Http;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Embeddings;

public static class MemoryEmbeddingsV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapPost(
                "memory/embeddings/upsert",
                async (
                    UpsertMemoryEmbeddingV1Request body,
                    IMemoryEmbeddingUpsertService upsert,
                    CancellationToken ct) =>
                {
                    var id = await upsert
                        .UpsertForMemoryItemAsync(
                            body.UserId ?? 0,
                            body.MemoryItemId,
                            body.EmbeddingModelKey ?? "",
                            body.EmbeddingModelVersion,
                            ct)
                        .ConfigureAwait(false);
                    return Results.Json(
                        new UpsertMemoryEmbeddingV1Response { EmbeddingRowId = id },
                        statusCode: StatusCodes.Status200OK);
                })
            .DisableAntiforgery();
    }
}
