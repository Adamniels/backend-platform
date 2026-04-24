namespace Platform.Domain.Features.Profile;

public sealed class PlatformProfile
{
    public const int SingletonKey = 1;

    public int Id { get; set; } = SingletonKey;
    public string DisplayName { get; set; } = "";
    public string Email { get; set; } = "";
}
