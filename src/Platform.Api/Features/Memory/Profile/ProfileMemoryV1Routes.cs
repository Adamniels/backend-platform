using Platform.Application.Features.Memory.Profile.GetProfileMemory;
using Platform.Application.Features.Memory.Profile.UpdateProfileMemory;
using Platform.Contracts.V1.Memory;

namespace Platform.Api.Features.Memory.Profile;

public static class ProfileMemoryV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "memory/explicit-profile",
            async (int? userId, GetProfileMemoryQueryHandler h, CancellationToken ct) =>
            {
                var res = await h
                    .HandleAsync(new GetProfileMemoryQuery(userId ?? 0), ct)
                    .ConfigureAwait(false);
                return Results.Ok(res);
            });

        v1.MapPut(
                "memory/explicit-profile",
                async (
                    UpdateProfileMemoryV1Request body,
                    int? userId,
                    UpdateProfileMemoryCommandHandler h,
                    CancellationToken ct) =>
                {
                    var command = new UpdateProfileMemoryCommand(
                        userId ?? 0,
                        body.CoreInterests ?? Array.Empty<string>(),
                        body.SecondaryInterests ?? Array.Empty<string>(),
                        body.Goals ?? Array.Empty<string>(),
                        body.Preferences ?? Array.Empty<ProfileMemoryPreferenceV1>(),
                        body.ActiveProjects ?? Array.Empty<ProfileMemoryProjectV1>(),
                        body.SkillLevels ?? Array.Empty<ProfileMemorySkillLevelV1>());
                    var res = await h
                        .HandleAsync(command, ct)
                        .ConfigureAwait(false);
                    return Results.Ok(res);
                })
            .DisableAntiforgery();
    }
}
