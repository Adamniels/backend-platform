using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions.Access;
using Platform.Application.Abstractions.Dashboard;
using Platform.Application.Abstractions.HumanInput;
using Platform.Application.Abstractions.News;
using Platform.Application.Abstractions.Profile;
using Platform.Application.Abstractions.SavedItems;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Abstractions.Stats;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Infrastructure.Access;
using Platform.Infrastructure.Configuration;
using Platform.Infrastructure.Features.Dashboard;
using Platform.Infrastructure.Features.HumanInput;
using Platform.Infrastructure.Features.Memory.DependencyInjection;
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
        services.AddPlatformMemoryInfrastructure();

        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform";

        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddSingleton<IWorkflowStartOptions, WorkflowStartOptions>();
        services.AddScoped<IAccessKeyValidationService, AccessKeyValidationService>();
        services.AddScoped<IDashboardReadModelSource, DashboardReadModelSource>();
        services.AddScoped<IStatsReadModelSource, StatsReadModelSource>();
        services.AddScoped<IWorkflowRunRepository, WorkflowRunRepository>();
        services.AddScoped<IProfileReadRepository, ProfileReadRepository>();
        services.AddScoped<INewsReadRepository, NewsReadRepository>();
        services.AddScoped<ISideLearningReadRepository, SideLearningReadRepository>();
        services.AddScoped<ISavedItemsReadRepository, SavedItemsReadRepository>();
        services.AddScoped<IHumanInputReadRepository, HumanInputReadRepository>();

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
