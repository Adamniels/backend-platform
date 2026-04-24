namespace Platform.Domain.Features.Profile;

public sealed class PlatformUserSettings
{
    public const int SingletonKey = 1;

    public int Id { get; set; } = SingletonKey;
    public string Theme { get; set; } = "system";
    public bool DigestEmail { get; set; }
}
