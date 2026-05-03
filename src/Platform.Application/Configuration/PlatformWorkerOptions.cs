namespace Platform.Application.Configuration;

/// <summary>Internal worker authentication (Bearer) and default user scope for worker-driven jobs.</summary>
public sealed class PlatformWorkerOptions
{
    public const string SectionName = "PlatformWorkers";

    /// <summary>Bearer token value expected on <c>Authorization: Bearer …</c> for <c>/api/internal/v1/*</c>. Empty disables internal routes (503).</summary>
    public string ServiceToken { get; set; } = "";

    /// <summary>Default user id when a request does not specify one (multi-user-ready; single-tenant default 1).</summary>
    public int PrimaryUserId { get; set; } = 1;
}
