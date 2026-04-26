using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions.Memory.Context;
using Platform.Application.Abstractions.Memory.Events;
using Platform.Application.Abstractions.Memory.Items;
using Platform.Application.Abstractions.Memory.Legacy;
using Platform.Application.Abstractions.Memory.Profile;
using Platform.Application.Abstractions.Memory.Procedural;
using Platform.Application.Abstractions.Memory.Review;
using Platform.Application.Abstractions.Memory.Semantic;
using Platform.Infrastructure.Features.Memory.Context;
using Platform.Infrastructure.Features.Memory.Events;
using Platform.Infrastructure.Features.Memory.Legacy;
using Platform.Infrastructure.Features.Memory.Profile;
using Platform.Infrastructure.Features.Memory.Review;
using Platform.Infrastructure.Features.Memory.Stubs;

namespace Platform.Infrastructure.Features.Memory.DependencyInjection;

public static class MemoryInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformMemoryInfrastructure(
        this IServiceCollection services) =>
        services
            .AddScoped<ILegacyMemoryInsightsReadRepository, LegacyMemoryInsightsReadRepository>()
            .AddScoped<IMemoryEventWriter, EfMemoryEventWriter>()
            .AddScoped<IMemoryItemReadRepository, MemoryItemReadRepositoryStub>()
            .AddScoped<ISemanticMemoryReadRepository, SemanticMemoryReadRepositoryStub>()
            .AddScoped<IProceduralRuleReadRepository, ProceduralRuleReadRepositoryStub>()
            .AddScoped<IMemoryContextProvider, EfMemoryContextProvider>()
            .AddScoped<IExplicitUserProfileRepository, EfExplicitUserProfileRepository>()
            .AddScoped<IMemoryReviewService, EfMemoryReviewService>()
            .AddScoped<ISemanticMemoryService, SemanticMemoryServiceShell>();
}
