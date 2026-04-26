namespace Platform.Application.Configuration;

/// <summary>Internal memory worker authentication and default consolidation scope.</summary>
public sealed class MemoryWorkerOptions
{
    public const string SectionName = "MemoryWorker";

    /// <summary>Bearer token value expected on <c>Authorization: Bearer …</c> for internal memory routes. Empty disables internal routes (503).</summary>
    public string ServiceToken { get; set; } = "";

    /// <summary>Default user id processed when a request does not specify one (multi-user-ready; single-tenant default 1).</summary>
    public int PrimaryUserId { get; set; } = 1;
}
