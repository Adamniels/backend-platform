using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Features.Memory.Context.GetMemoryContextShell;
using Platform.Application.Features.Memory.Context.GetMemoryContextV1;
using Platform.Application.Features.Memory.Events.ListMemoryEvents;
using Platform.Application.Features.Memory.Documents.IngestDocumentMemory;
using Platform.Application.Features.Memory.Events.IngestEvent;
using Platform.Application.Features.Memory.Items.ListItems;
using Platform.Application.Features.Memory.Legacy.Insights;
using Platform.Application.Features.Memory.Profile.GetProfileMemory;
using Platform.Application.Features.Memory.Procedural.ActivateProceduralRule;
using Platform.Application.Features.Memory.Procedural.CreateProceduralRule;
using Platform.Application.Features.Memory.Procedural.DeprecateProceduralRule;
using Platform.Application.Features.Memory.Procedural.GetProceduralRule;
using Platform.Application.Features.Memory.Procedural.ListProceduralRules;
using Platform.Application.Features.Memory.Procedural.PublishProceduralRuleVersion;
using Platform.Application.Features.Memory.Procedural.UpdateProceduralRulePriority;
using Platform.Application.Features.Memory.Profile.UpdateProfileMemory;
using Platform.Application.Features.Memory.ReviewQueue.ApproveItem;
using Platform.Application.Features.Memory.ReviewQueue.CreateItem;
using Platform.Application.Features.Memory.ReviewQueue.ListPending;
using Platform.Application.Features.Memory.ReviewQueue.PatchItem;
using Platform.Application.Features.Memory.ReviewQueue.RejectItem;
using Platform.Application.Features.Memory.Semantic.ArchiveSemanticMemory;
using Platform.Application.Features.Memory.Semantic.AttachSemanticMemoryEvidence;
using Platform.Application.Features.Memory.Semantic.CreateSemanticMemory;
using Platform.Application.Features.Memory.Semantic.ListSemanticMemoryEvidence;
using Platform.Application.Features.Memory.Semantic.FindSimilarSemanticMemories;
using Platform.Application.Features.Memory.Semantic.GetSemanticMemory;
using Platform.Application.Features.Memory.Semantic.ListSemanticMemories;
using Platform.Application.Features.Memory.Semantic.RejectSemanticMemory;
using Platform.Application.Features.Memory.Consolidation.Nightly;
using Platform.Application.Features.Memory.Semantic.UpdateSemanticMemoryConfidence;

namespace Platform.Application.Features.Memory.DependencyInjection;

public static class MemoryApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryApplication(this IServiceCollection services) =>
        services
            .AddScoped<ListMemoryInsightsQueryHandler>()
            .AddScoped<IngestMemoryEventCommandHandler>()
            .AddScoped<ListMemoryEventsQueryHandler>()
            .AddScoped<ListSemanticMemoryEvidenceQueryHandler>()
            .AddScoped<ListMemoryItemsQueryHandler>()
            .AddScoped<GetMemoryContextQueryHandler>()
            .AddScoped<ListMemoryReviewQueueQueryHandler>()
            .AddScoped<CreateReviewQueueItemCommandHandler>()
            .AddScoped<ApproveReviewQueueItemCommandHandler>()
            .AddScoped<RejectReviewQueueItemCommandHandler>()
            .AddScoped<PatchReviewQueueItemCommandHandler>()
            .AddScoped<GetProfileMemoryQueryHandler>()
            .AddScoped<UpdateProfileMemoryCommandHandler>()
            .AddScoped<PostMemoryContextRequestHandler>()
            .AddScoped<IngestDocumentMemoryCommandHandler>()
            .AddScoped<CreateSemanticMemoryCommandHandler>()
            .AddScoped<UpdateSemanticMemoryConfidenceCommandHandler>()
            .AddScoped<AttachSemanticMemoryEvidenceCommandHandler>()
            .AddScoped<ArchiveSemanticMemoryCommandHandler>()
            .AddScoped<RejectSemanticMemoryCommandHandler>()
            .AddScoped<ListSemanticMemoriesQueryHandler>()
            .AddScoped<GetSemanticMemoryQueryHandler>()
            .AddScoped<FindSimilarSemanticMemoriesQueryHandler>()
            .AddScoped<ExecuteNightlyMemoryConsolidationCommandHandler>()
            .AddScoped<ListProceduralRulesQueryHandler>()
            .AddScoped<GetProceduralRuleQueryHandler>()
            .AddScoped<CreateProceduralRuleCommandHandler>()
            .AddScoped<PublishProceduralRuleVersionCommandHandler>()
            .AddScoped<UpdateProceduralRulePriorityCommandHandler>()
            .AddScoped<ActivateProceduralRuleCommandHandler>()
            .AddScoped<DeprecateProceduralRuleCommandHandler>();
}
