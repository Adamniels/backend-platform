namespace Platform.Domain.Features.Dashboard;

/// <summary>
/// Single-row cache for stats payload JSON (matches frontend StatsPayload shape).
/// </summary>
public sealed class StatsSnapshot
{
    public const int SingletonKey = 1;

    public int Id { get; set; } = SingletonKey;
    public string Json { get; set; } = "{}";
}
