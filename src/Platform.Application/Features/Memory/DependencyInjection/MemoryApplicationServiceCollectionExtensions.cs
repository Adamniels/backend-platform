using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Features.Memory.Context.GetMemoryContextShell;
using Platform.Application.Features.Memory.Events.IngestEvent;
using Platform.Application.Features.Memory.Items.ListItems;
using Platform.Application.Features.Memory.Legacy.Insights;
using Platform.Application.Features.Memory.ReviewQueue.ListPending;

namespace Platform.Application.Features.Memory.DependencyInjection;

public static class MemoryApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryApplication(this IServiceCollection services) =>
        services
            .AddScoped<ListMemoryInsightsQueryHandler>()
            .AddScoped<IngestMemoryEventCommandHandler>()
            .AddScoped<ListMemoryItemsQueryHandler>()
            .AddScoped<GetMemoryContextQueryHandler>()
            .AddScoped<ListMemoryReviewQueueQueryHandler>();
}
