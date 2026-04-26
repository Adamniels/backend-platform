using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Platform.IntegrationTests.Infrastructure;

namespace Platform.IntegrationTests;

public sealed class PlatformWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string LocalComposeConnection =
        "Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var cs = string.IsNullOrWhiteSpace(MemoryPostgresConnection.ConnectionString)
            ? LocalComposeConnection
            : MemoryPostgresConnection.ConnectionString;
        builder.UseSetting("ConnectionStrings:Default", cs);
        builder.UseSetting("Platform:AccessKey", "integration-test-access-key");
        builder.UseSetting("Platform:CookieSecure", "false");
        builder.UseSetting("Platform:PublicHealth", "false");
        builder.UseSetting("MemoryWorker:ServiceToken", "integration-memory-worker-token");
        builder.UseSetting("MemoryWorker:PrimaryUserId", "1");
    }
}
