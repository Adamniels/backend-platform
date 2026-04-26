namespace Platform.Application.Configuration;

public sealed class PlatformAccessOptions
{
    public const string SectionName = "Platform";

    /// <summary>Static key compared at unlock (env override recommended).</summary>
    public string AccessKey { get; set; } = "";

    public string CookieName { get; set; } = "platform_session";
    public int SessionHours { get; set; } = 24;
    public bool CookieSecure { get; set; } = true;
    public bool PublicHealth { get; set; }
    public string[] AllowedOrigins { get; set; } = ["http://localhost:3000", "https://localhost:3000"];

    /// <summary>Optional directory for Data Protection key ring persistence.</summary>
    public string? DataProtectionKeysPath { get; set; }
}
