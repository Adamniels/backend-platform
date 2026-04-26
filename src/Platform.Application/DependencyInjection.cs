using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Features.Access.UnlockSession;
using Platform.Application.Features.Dashboard.GetSummary;
using Platform.Application.Features.HumanInput.ListItems;
using Platform.Application.Features.Memory.DependencyInjection;
using Platform.Application.Features.News.ListFeed;
using Platform.Application.Features.Profile.GetProfile;
using Platform.Application.Features.Profile.GetSettings;
using Platform.Application.Features.SavedItems.ListSavedItems;
using Platform.Application.Features.SideLearning.ListTopics;
using Platform.Application.Features.Stats.GetStats;
using Platform.Application.Features.WorkflowRuns.ListWorkflowRuns;
using Platform.Application.Features.WorkflowRuns.StartWorkflowRun;

namespace Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        return services
            .AddMemoryApplication()
            .AddScoped<GetDashboardSummaryQueryHandler>()
            .AddScoped<GetStatsQueryHandler>()
            .AddScoped<ListWorkflowRunsQueryHandler>()
            .AddScoped<StartWorkflowRunCommandHandler>()
            .AddScoped<GetProfileQueryHandler>()
            .AddScoped<GetUserSettingsQueryHandler>()
            .AddScoped<ListNewsFeedQueryHandler>()
            .AddScoped<ListSideLearningTopicsQueryHandler>()
            .AddScoped<ListSavedItemsQueryHandler>()
            .AddScoped<ListHumanInputQueryHandler>()
            .AddScoped<UnlockSessionCommandHandler>();
    }
}
