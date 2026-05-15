using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions.Memory.Consolidation;
using Platform.Application.Abstractions.Memory.Confidence;
using Platform.Application.Abstractions.Memory.Contradictions;
using Platform.Application.Abstractions.Memory.Documents;
using Platform.Application.Abstractions.Memory.Context;
using Platform.Application.Abstractions.Memory.Events;
using Platform.Application.Abstractions.Memory.Evidence;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.Memory.Items;
using Platform.Application.Abstractions.Memory.Maintenance;
using Platform.Application.Features.Memory.Embeddings;
using Platform.Application.Abstractions.Memory.Profile;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Infrastructure.Features.Memory.Consolidation;
using Platform.Infrastructure.Features.Memory.Confidence;
using Platform.Infrastructure.Features.Memory.Contradictions;
using Platform.Infrastructure.Features.Memory.Documents;
using Platform.Infrastructure.Features.Memory.Context;
using Platform.Infrastructure.Features.Memory.Embeddings;
using Platform.Infrastructure.Features.Memory.Events;
using Platform.Infrastructure.Features.Memory.Evidence;
using Platform.Infrastructure.Features.Memory.Items;
using Platform.Infrastructure.Features.Memory.Maintenance;
using Platform.Infrastructure.Features.Memory.Procedural;
using Platform.Infrastructure.Features.Memory.Profile;
using Platform.Infrastructure.Features.Memory.Review;
using Platform.Infrastructure.Features.Memory.Review.Approval;
using Platform.Infrastructure.Features.Memory.Semantic;
using Platform.Infrastructure.Features.Memory.Users;

namespace Platform.Infrastructure.Features.Memory.DependencyInjection;

public static class MemoryInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformMemoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<MemoryVectorRetrievalOptions>()
            .Bind(configuration.GetSection(MemoryVectorRetrievalOptions.SectionName));
        services.AddOptions<DocumentMemoryChunkingOptions>()
            .Bind(configuration.GetSection("DocumentMemory"));

        // Note: IMemoryEmbeddingGenerator is registered in the top-level DependencyInjection as
        // a Singleton (OpenAiEmbeddingGenerator) so that both the news and memory pipelines share
        // one implementation. No registration here.

        return services
            .AddScoped<IMemoryVectorRecallSearch, EfMemoryVectorRecallSearch>()
            .AddScoped<IMemoryEmbeddingUpsertService, EfMemoryEmbeddingUpsertService>()
            .AddScoped<IDocumentMemoryIngestService, EfDocumentMemoryIngestService>()
            .AddScoped<IMemoryEventWriter, EfMemoryEventWriter>()
            .AddScoped<IMemoryEventsReadRepository, EfMemoryEventsReadRepository>()
            .AddScoped<IMemoryEvidenceReadRepository, EfMemoryEvidenceReadRepository>()
            .AddScoped<IMemoryConsolidationRunRepository, EfMemoryConsolidationRunRepository>()
            .AddSingleton<IMemoryConsolidationPolicyProvider, DefaultMemoryConsolidationPolicyProvider>()
            .AddSingleton<IMemoryConfidencePolicy, DefaultMemoryConfidencePolicy>()
            .AddSingleton<IMemoryEventPolicyProvider, DefaultMemoryEventPolicyProvider>()
            .AddSingleton<IExplicitProfileConflictDetector, ExplicitProfileConflictDetector>()
            .AddScoped<ISemanticConflictEvaluationService, EfSemanticConflictEvaluationService>()
            .AddScoped<IMemoryItemReadRepository, EfMemoryItemReadRepository>()
            .AddScoped<ISemanticMemoryReadRepository, EfSemanticMemoryReadRepository>()
            .AddScoped<EfProceduralRuleService>()
            .AddScoped<IProceduralRuleService>(sp => sp.GetRequiredService<EfProceduralRuleService>())
            .AddScoped<IProceduralRuleReadRepository>(sp => sp.GetRequiredService<EfProceduralRuleService>())
            .AddScoped<IMemoryContextProvider, EfMemoryContextProvider>()
            .AddScoped<IExplicitUserProfileRepository, EfExplicitUserProfileRepository>()
            .AddScoped<IMemoryUserContextResolver, DefaultMemoryUserContextResolver>()
            .AddScoped<IMemoryReviewApprovalHandler, NewSemanticApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, NewProceduralRuleApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, ArchiveStaleSemanticApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, MergeSemanticCandidatesApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, ContradictionDetectedApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, ConflictWithExplicitProfileApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, SupersedeSemanticApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, ReviseSemanticClaimApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandler, ReviseProceduralRuleApprovalHandler>()
            .AddScoped<IMemoryReviewApprovalHandlerResolver, MemoryReviewApprovalHandlerResolver>()
            .AddScoped<IMemoryReviewService, EfMemoryReviewService>()
            .AddScoped<ISemanticMemoryService, EfSemanticMemoryService>()
            .AddScoped<IMemorySemanticMergeService, EfMemorySemanticMergeService>()
            .AddScoped<ISemanticConfidenceRecomputeService, EfSemanticConfidenceRecomputeService>()
            .AddSingleton<IStaleSemanticPolicy, DefaultStaleSemanticPolicy>()
            .AddSingleton<IContradictionEvaluationService, DefaultContradictionEvaluationService>()
            .AddSingleton<ISemanticDuplicateDetector, DefaultSemanticDuplicateDetector>()
            .AddScoped<IMemoryReviewProposalEmitter, MemoryReviewProposalEmitter>()
            .AddScoped<ISemanticMemoryMaintenanceService, EfSemanticMemoryMaintenanceService>();
    }
}
