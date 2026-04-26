using Platform.Api.Features.Memory.Events;
using Platform.Api.Features.Memory.Legacy.Insights;
using Platform.Api.Features.Memory.Module;

namespace Platform.Api.Features.Memory;

public static class MemoryV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        MemoryModuleV1Routes.Map(v1);
        MemoryEventsV1Routes.Map(v1);
        MemoryInsightsV1Routes.Map(v1);
    }
}
