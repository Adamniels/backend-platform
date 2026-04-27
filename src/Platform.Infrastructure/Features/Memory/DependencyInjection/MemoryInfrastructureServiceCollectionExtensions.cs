using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.Memory.Consolidation;
using Platform.Application.Abstractions.Memory.Confidence;
using Platform.Application.Abstractions.Memory.Contradictions;
using Platform.Application.Abstractions.Memory.Documents;
using Platform.Application.Abstractions.Memory.Context;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.Memory.Events;
using Platform.Application.Abstractions.Memory.Evidence;
using Platform.Application.Abstractions.Memory.Items;
using Platform.Application.Abstractions.Memory.Legacy;
using Platform.Application.Abstractions.Memory.Maintenance;
using Platform.Application.Abstractions.Memory.Profile;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Application.Abstractions.Memory.Users;
using Platform.Application.Features.Memory.Embeddings;
using Platform.Infrastructure.Features.Memory.Consolidation;
using Platform.Infrastructure.Features.Memory.Confidence;
using Platform.Infrastructure.Features.Memory.Contradictions;
using Platform.Infrastructure.Features.Memory.Documents;
using Platform.Infrastructure.Features.Memory.Context;
using Platform.Infrastructure.Features.Memory.Embeddings;
using Platform.Infrastructure.Features.Memory.Events;
using Platform.Infrastructure.Features.Memory.Evidence;
using Platform.Infrastructure.Features.Memory.Legacy;
using Platform.Infrastructure.Features.Memory.Maintenance;
using Platform.Infrastructure.Features.Memory.Procedural;
using Platform.Infrastructure.Features.Memory.Profile;
using Platform.Infrastructure.Features.Memory.Review;
using Platform.Infrastructure.Features.Memory.Review.Approval;
using Platform.Infrastructure.Features.Memory.Semantic;
using Platform.Infrastructure.Features.Memory.Stubs;
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
        return services
            .AddSingleton<IMemoryEmbeddingGenerator>(
                sp =>
                {
                    var env = sp.GetRequiredService<IHostEnvironment>();
                    var opts = sp.GetService<IOptions<MemoryVectorRetrievalOptions>>()?.Value;
                    var useDeterministic = opts?.UseDeterministicEmbeddingGenerator == true ||
                        string.Equals(
                            env.EnvironmentName,
                            "Testing",
                            StringComparison.OrdinalIgnoreCase);
                    return useDeterministic
                        ? new DeterministicRecallEmbeddingGenerator()
                        : new NoOpMemoryEmbeddingGenerator();
                })
            .AddScoped<IMemoryVectorRecallSearch, EfMemoryVectorRecallSearch>()
            .AddScoped<IMemoryEmbeddingUpsertService, EfMemoryEmbeddingUpsertService>()
            .AddScoped<IDocumentMemoryIngestService, EfDocumentMemoryIngestService>()
            .AddScoped<ILegacyMemoryInsightsReadRepository, LegacyMemoryInsightsReadRepository>()
            .AddScoped<IMemoryEventWriter, EfMemoryEventWriter>()
            .AddScoped<IMemoryEventsReadRepository, EfMemoryEventsReadRepository>()
            .AddScoped<IMemoryEvidenceReadRepository, EfMemoryEvidenceReadRepository>()
            .AddScoped<IMemoryConsolidationRunRepository, EfMemoryConsolidationRunRepository>()
            .AddSingleton<IMemoryConsolidationPolicyProvider, DefaultMemoryConsolidationPolicyProvider>()
            .AddSingleton<IMemoryConfidencePolicy, DefaultMemoryConfidencePolicy>()
            .AddSingleton<IMemoryEventPolicyProvider, DefaultMemoryEventPolicyProvider>()
            .AddSingleton<IExplicitProfileConflictDetector, ExplicitProfileConflictDetector>()
            .AddScoped<ISemanticConflictEvaluationService, EfSemanticConflictEvaluationService>()
            .AddScoped<IMemoryItemReadRepository, MemoryItemReadRepositoryStub>()
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
