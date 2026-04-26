namespace Platform.Application.Features.Memory.Semantic.FindSimilarSemanticMemories;

public sealed record FindSimilarSemanticMemoriesQuery(
    int UserId,
    string? KeySubstring,
    string? Domain,
    int Take);
