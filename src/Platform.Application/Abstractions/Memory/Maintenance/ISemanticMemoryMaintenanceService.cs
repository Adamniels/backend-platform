namespace Platform.Application.Abstractions.Memory.Maintenance;

public interface ISemanticMemoryMaintenanceService
{
    Task<SemanticMemoryMaintenanceOutcome> RunAsync(
        int userId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);
}

public sealed record SemanticMemoryMaintenanceOutcome(
    int RecomputedSemanticsCount,
    int StaleProposalsCreatedCount,
    int ContradictionProposalsCreatedCount,
    int MergeProposalsCreatedCount);
