namespace Platform.Application.Features.News.Profile;

public sealed record UpdateNewsProfileCommand(
    int UserId,
    int WindowDays = 7);

public enum UpdateNewsProfileResult
{
    /// <summary>Profile embedding was recalculated and saved.</summary>
    Updated,

    /// <summary>No interactions in the window — nothing to compute.</summary>
    NoData,

    /// <summary>No profile exists yet — Phase 2 seeding has not run for this user.</summary>
    NoProfile,

    /// <summary>A non-recoverable error occurred (e.g. embedding generator unavailable).</summary>
    Error,
}
