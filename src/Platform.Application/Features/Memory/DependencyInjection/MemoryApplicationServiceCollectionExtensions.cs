using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Features.Memory.Context.GetMemoryContextShell;
using Platform.Application.Features.Memory.Context.GetMemoryContextV1;
using Platform.Application.Features.Memory.Events.IngestEvent;
using Platform.Application.Features.Memory.Items.ListItems;
using Platform.Application.Features.Memory.Legacy.Insights;
using Platform.Application.Features.Memory.Profile.GetProfileMemory;
using Platform.Application.Features.Memory.Profile.UpdateProfileMemory;
using Platform.Application.Features.Memory.ReviewQueue.ApproveItem;
using Platform.Application.Features.Memory.ReviewQueue.CreateItem;
using Platform.Application.Features.Memory.ReviewQueue.ListPending;
using Platform.Application.Features.Memory.ReviewQueue.PatchItem;
using Platform.Application.Features.Memory.ReviewQueue.RejectItem;

namespace Platform.Application.Features.Memory.DependencyInjection;

public static class MemoryApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMemoryApplication(this IServiceCollection services) =>
        services
            .AddScoped<ListMemoryInsightsQueryHandler>()
            .AddScoped<IngestMemoryEventCommandHandler>()
            .AddScoped<ListMemoryItemsQueryHandler>()
            .AddScoped<GetMemoryContextQueryHandler>()
            .AddScoped<ListMemoryReviewQueueQueryHandler>()
            .AddScoped<CreateReviewQueueItemCommandHandler>()
            .AddScoped<ApproveReviewQueueItemCommandHandler>()
            .AddScoped<RejectReviewQueueItemCommandHandler>()
            .AddScoped<PatchReviewQueueItemCommandHandler>()
            .AddScoped<GetProfileMemoryQueryHandler>()
            .AddScoped<UpdateProfileMemoryCommandHandler>()
            .AddScoped<PostMemoryContextRequestHandler>();
}
