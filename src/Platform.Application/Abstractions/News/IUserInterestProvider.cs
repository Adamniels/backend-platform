namespace Platform.Application.Abstractions.News;

/// <summary>News-facing read model of user-declared interests (backed by explicit profile memory in v1).</summary>
public sealed record UserInterestProjectSnapshot(string Name, string? ExternalId);

public sealed record UserInterestSnapshot(
    IReadOnlyList<string> CoreInterests,
    IReadOnlyList<string> SecondaryInterests,
    IReadOnlyList<string> Goals,
    IReadOnlyList<UserInterestProjectSnapshot> ActiveProjects);

/// <summary>Port for news ranking and workers to read interests without depending on Memory feature internals.</summary>
public interface IUserInterestProvider
{
    Task<UserInterestSnapshot> GetInterestsAsync(int userId, CancellationToken cancellationToken = default);
}
