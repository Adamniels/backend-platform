using Platform.Application.Features.Memory.Context;
using Platform.Contracts.V1.Memory;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Semantic;

public static class SemanticMemoryV1Mapper
{
    public static SemanticMemoryV1Dto ToV1Dto(this SemanticMemory s) =>
        new()
        {
            Id = s.Id,
            Key = s.Key,
            Claim = s.Claim,
            Domain = s.Domain,
            Confidence = s.Confidence,
            AuthorityWeight = s.AuthorityWeight,
            Status = MemoryContextV1Scoring.SemanticStatusString(s.Status),
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
            LastSupportedAt = s.LastSupportedAt,
        };
}
