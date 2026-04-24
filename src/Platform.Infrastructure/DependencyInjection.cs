using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Features.Dashboard;
using Platform.Application.Features.HumanInput;
using Platform.Application.Features.Memory;
using Platform.Application.Features.News;
using Platform.Application.Features.Profile;
using Platform.Application.Features.SavedItems;
using Platform.Application.Features.SideLearning;
using Platform.Application.Features.WorkflowRuns;
using Platform.Infrastructure.Features.Dashboard;
using Platform.Infrastructure.Features.HumanInput;
using Platform.Infrastructure.Features.Memory;
using Platform.Infrastructure.Features.News;
using Platform.Infrastructure.Features.Profile;
using Platform.Infrastructure.Features.SavedItems;
using Platform.Infrastructure.Features.SideLearning;
using Platform.Infrastructure.Features.WorkflowRuns;
using Platform.Infrastructure.Persistence;
using Platform.Infrastructure.Temporal;

namespace Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform";

        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDashboardQueries, DashboardQueries>();
        services.AddScoped<IStatsQueries, StatsQueries>();
        services.AddScoped<IWorkflowRunQueries, WorkflowRunQueries>();
        services.AddScoped<IWorkflowRunCommands, WorkflowRunCommands>();
        services.AddScoped<IProfileQueries, ProfileQueries>();
        services.AddScoped<INewsQueries, NewsQueries>();
        services.AddScoped<ISideLearningQueries, SideLearningQueries>();
        services.AddScoped<ISavedItemQueries, SavedItemQueries>();
        services.AddScoped<IMemoryQueries, MemoryQueries>();
        services.AddScoped<IHumanInputQueries, HumanInputQueries>();

        var temporalAddress = configuration["Temporal:Address"];
        if (string.IsNullOrWhiteSpace(temporalAddress))
        {
            services.AddSingleton<IWorkflowStarter, StubWorkflowStarter>();
        }
        else
        {
            services.AddSingleton<IWorkflowStarter, TemporalWorkflowStarter>();
        }

        return services;
    }
}
