using Microsoft.AspNetCore.Http;
using Platform.Application.Features.Memory.Documents.IngestDocumentMemory;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Documents;

public static class DocumentMemoryV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapPost(
                "memory/documents",
                async (
                    IngestDocumentMemoryV1Request body,
                    IngestDocumentMemoryCommandHandler h,
                    CancellationToken ct) =>
                {
                    var res = await h.HandleAsync(body, ct).ConfigureAwait(false);
                    return Results.Json(res, statusCode: StatusCodes.Status200OK);
                })
            .DisableAntiforgery();
    }
}
