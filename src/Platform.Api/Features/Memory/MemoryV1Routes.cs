using Platform.Api.Features.Memory.Context;
using Platform.Api.Features.Memory.Embeddings;
using Platform.Api.Features.Memory.Events;
using Platform.Api.Features.Memory.Legacy.Insights;
using Platform.Api.Features.Memory.Module;
using Platform.Api.Features.Memory.Profile;
using Platform.Api.Features.Memory.Review;
using Platform.Api.Features.Memory.Procedural;
using Platform.Api.Features.Memory.Semantic;

namespace Platform.Api.Features.Memory;

public static class MemoryV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        MemoryModuleV1Routes.Map(v1);
        MemoryEventsV1Routes.Map(v1);
        MemoryContextV1Routes.Map(v1);
        MemoryEmbeddingsV1Routes.Map(v1);
        MemoryReviewV1Routes.Map(v1);
        ProfileMemoryV1Routes.Map(v1);
        SemanticMemoryV1Routes.Map(v1);
        ProceduralMemoryV1Routes.Map(v1);
        MemoryInsightsV1Routes.Map(v1);
    }
}
