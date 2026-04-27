namespace Platform.Application.Abstractions.Memory.Contradictions;

public interface ISemanticConflictEvaluationService
{
    Task<bool> ConflictsWithExplicitProfileAsync(
        int userId,
        long semanticMemoryId,
        CancellationToken cancellationToken = default);
}
