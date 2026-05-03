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
using Platform.Application.Features.SideLearning.Internal.PostReflectionInsights;
using Platform.Application.Features.SideLearning.Internal.PostSessionContent;
using Platform.Application.Features.SideLearning.Internal.PostTopicProposals;
using Platform.Application.Features.SideLearning.Sessions.Create;
using Platform.Application.Features.SideLearning.Sessions.Get;
using Platform.Application.Features.SideLearning.Sessions.List;
using Platform.Application.Features.SideLearning.Sessions.Progress;
using Platform.Application.Features.SideLearning.Sessions.Reflect;
using Platform.Application.Features.SideLearning.Sessions.SelectTopic;
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
            .AddScoped<CreateSideLearningSessionCommandHandler>()
            .AddScoped<GetSideLearningSessionQueryHandler>()
            .AddScoped<ListSideLearningSessionsQueryHandler>()
            .AddScoped<SelectSideLearningTopicCommandHandler>()
            .AddScoped<UpdateSideLearningProgressCommandHandler>()
            .AddScoped<SubmitSideLearningReflectionCommandHandler>()
            .AddScoped<PostSideLearningTopicProposalsCommandHandler>()
            .AddScoped<PostSideLearningSessionContentCommandHandler>()
            .AddScoped<PostSideLearningReflectionInsightsCommandHandler>()
            .AddScoped<ListSavedItemsQueryHandler>()
            .AddScoped<ListHumanInputQueryHandler>()
            .AddScoped<UnlockSessionCommandHandler>();
    }
}
