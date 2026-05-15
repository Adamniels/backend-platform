using Platform.Api.Features.Memory.Internal;
using Platform.Api.Features.News.Internal;
using Platform.Api.Features.SideLearning.Internal;

namespace Platform.Api.Features;

public static class InternalApiRegistration
{
    public static void MapInternalEndpoints(this WebApplication app)
    {
        InternalMemoryV1Routes.Map(app);
        InternalNewsV1Routes.Map(app);
        InternalSideLearningV1Routes.Map(app);
    }
}
