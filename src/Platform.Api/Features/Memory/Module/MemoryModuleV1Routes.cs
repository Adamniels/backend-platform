using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Module;

public static class MemoryModuleV1Routes
{
    public static void Map(RouteGroupBuilder v1) =>
        v1.MapGet(
            "memory/structure",
            () => Results.Ok(
                new MemoryModuleDescriptorV1Dto(
                    StructureVersion: 1,
                    BoundedContextAreas:
                    [
                        "Users",
                        "Items",
                        "Events",
                        "Semantic",
                        "Procedural",
                        "ReviewQueue",
                        "GraphRelationship",
                    ])));
}
