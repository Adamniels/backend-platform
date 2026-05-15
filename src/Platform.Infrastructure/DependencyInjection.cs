using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform.Application.Abstractions;
using Platform.Application.Abstractions.Access;
using Platform.Application.Abstractions.Dashboard;
using Platform.Application.Abstractions.HumanInput;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Abstractions.News;
using Platform.Application.Abstractions.Profile;
using Platform.Application.Abstractions.SavedItems;
using Platform.Application.Abstractions.SideLearning;
using Platform.Application.Abstractions.Stats;
using Platform.Application.Abstractions.Workflows;
using Platform.Application.Abstractions.WorkflowRuns;
using Platform.Infrastructure.AI;
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
using Platform.Infrastructure.Health;
using Platform.Infrastructure.Persistence;
using Platform.Infrastructure.Temporal;

namespace Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPlatformMemoryInfrastructure(configuration);

        // C6: Fail fast on missing connection string outside Development / Testing.
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
            var isDev = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase)
                     || string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase);

            if (!isDev)
                throw new InvalidOperationException(
                    "ConnectionStrings:Default is required in non-Development environments.");

            connectionString = "Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform";
        }

        services.AddDbContext<PlatformDbContext>(
            options => options.UseNpgsql(connectionString, o => o.UseVector()));

        // C3: Health and migration ports — Api host uses these instead of PlatformDbContext directly.
        services.AddScoped<IDatabaseHealthCheck, EfDatabaseHealthCheck>();
        services.AddScoped<IDatabaseMigrator, EfDatabaseMigrator>();

        services.AddHttpClient();

        // C1: Singleton — OpenAiEmbeddingGenerator holds no request-scoped state; IHttpClientFactory
        // is Singleton-safe. This is the single registration for IMemoryEmbeddingGenerator; the
        // memory infrastructure extension no longer registers it.
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionKey));
        services.AddSingleton<IMemoryEmbeddingGenerator, OpenAiEmbeddingGenerator>();

        services.AddSingleton<IWorkflowStartOptions, WorkflowStartOptions>();
        services.AddScoped<IAccessKeyValidationService, AccessKeyValidationService>();
        services.AddScoped<IDashboardReadModelSource, DashboardReadModelSource>();
        services.AddScoped<IStatsReadModelSource, StatsReadModelSource>();
        services.AddScoped<IWorkflowRunRepository, WorkflowRunRepository>();
        services.AddScoped<IProfileReadRepository, ProfileReadRepository>();
        services.AddScoped<INewsReadRepository, NewsReadRepository>();
        services.AddScoped<INewsIngestRepository, EfNewsIngestRepository>();
        services.AddScoped<INewsDeleteRepository, EfNewsDeleteRepository>();
        services.AddScoped<INewsEmbeddingRepository, EfNewsEmbeddingRepository>();
        services.AddScoped<INewsProfileRepository, EfNewsProfileRepository>();
        services.AddScoped<INewsVectorSearch, NewsVectorSearch>();
        services.AddScoped<IUserInterestProvider, ExplicitProfileUserInterestProvider>();
        services.AddScoped<ISideLearningSessionRepository, SideLearningSessionRepository>();
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
