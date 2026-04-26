using Platform.Application.Features.Profile.GetProfile;
using Platform.Application.Features.Profile.GetSettings;

namespace Platform.Api.Features.Profile;

public static class ProfileV1Routes
{
    public static void Map(RouteGroupBuilder v1)
    {
        v1.MapGet(
            "profile",
            async (GetProfileQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new GetProfileQuery(), ct)
                        .ConfigureAwait(false)));

        v1.MapGet(
            "settings",
            async (GetUserSettingsQueryHandler h, CancellationToken ct) =>
                Results.Ok(
                    await h
                        .HandleAsync(new GetUserSettingsQuery(), ct)
                        .ConfigureAwait(false)));
    }
}
