namespace Platform.Application.Features.Memory.Semantic.RejectSemanticMemory;

public sealed record RejectSemanticMemoryCommand(long SemanticMemoryId, int UserId);
