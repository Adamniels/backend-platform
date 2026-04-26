using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions.Memory.Context;
using Platform.Application.Abstractions.Memory.Events;
using Platform.Application.Abstractions.Memory.Items;
using Platform.Application.Abstractions.Memory.Legacy;
using Platform.Application.Abstractions.Memory.Profile;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Infrastructure.Features.Memory.Legacy;
using Platform.Infrastructure.Features.Memory.Stubs;

namespace Platform.Infrastructure.Features.Memory.DependencyInjection;

public static class MemoryInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformMemoryInfrastructure(
        this IServiceCollection services) =>
        services
            .AddScoped<ILegacyMemoryInsightsReadRepository, LegacyMemoryInsightsReadRepository>()
            .AddScoped<IMemoryEventWriter, NoOpMemoryEventWriter>()
            .AddScoped<IMemoryItemReadRepository, MemoryItemReadRepositoryStub>()
            .AddScoped<ISemanticMemoryReadRepository, SemanticMemoryReadRepositoryStub>()
            .AddScoped<IProceduralRuleReadRepository, ProceduralRuleReadRepositoryStub>()
            .AddScoped<IMemoryReviewQueueReadRepository, MemoryReviewQueueReadRepositoryStub>()
            .AddScoped<IMemoryContextProvider, MemoryContextProviderShell>()
            .AddScoped<IMemoryProfileService, MemoryProfileServiceShell>()
            .AddScoped<IMemoryReviewService, MemoryReviewServiceShell>()
            .AddScoped<ISemanticMemoryService, SemanticMemoryServiceShell>();
}
