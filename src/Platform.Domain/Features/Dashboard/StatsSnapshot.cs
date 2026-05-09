namespace Platform.Domain.Features.Dashboard;

/// <summary>
/// Persisted stats payload JSON (matches frontend StatsPayload shape). Baseline row is empty arrays; updated when a stats pipeline exists.
/// </summary>
public sealed class StatsSnapshot
{
    public const int SingletonKey = 1;

    public int Id { get; set; } = SingletonKey;
    public string Json { get; set; } = "{}";
}
