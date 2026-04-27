using FluentValidation;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Contracts.V1.Memory;
using Platform.Application.Features.Memory.Semantic;

namespace Platform.Application.Features.Memory.Semantic.CreateSemanticMemory;

public sealed class CreateSemanticMemoryCommandHandler(
    IValidator<CreateSemanticMemoryCommand> validator,
    ISemanticMemoryService semantics,
    IMemoryUserContextResolver userResolver)
{
    public async Task<SemanticMemoryV1Dto> HandleAsync(
        CreateSemanticMemoryCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);
        var userId = userResolver.Resolve(command.UserId);
        var initial = SemanticMemoryInitialStatus.Parse(command.Status);
        var auth = command.AuthorityWeight
            ?? global::Platform.Domain.Features.Memory.ValueObjects.AuthorityWeight.Inferred.Value;
        var created = await semantics
            .CreateWithInitialEvidenceAsync(
                userId,
                command.Key,
                command.Claim,
                command.Confidence,
                auth,
                command.Domain,
                initial,
                command.EventId,
                command.EvidenceStrength,
                command.EvidenceReason,
                SemanticEvidenceContractParser.ParsePolarity(command.EvidencePolarity),
                SemanticEvidenceContractParser.ParseSourceKind(command.EvidenceSourceKind),
                command.EvidenceReliabilityWeight ?? 0.55d,
                command.EvidenceSourceId,
                command.EvidenceSchemaVersion,
                command.EvidenceProvenanceJson,
                cancellationToken)
            .ConfigureAwait(false);
        return created.ToV1Dto();
    }
}
