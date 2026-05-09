namespace Platform.Domain.Features.Dashboard;

/// <summary>
/// Optional persisted stats payload JSON (matches frontend StatsPayload shape). Not seeded; populated when a stats pipeline exists.
/// </summary>
public sealed class StatsSnapshot
{
    public const int SingletonKey = 1;

    public int Id { get; set; } = SingletonKey;
    public string Json { get; set; } = "{}";
}
