using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Platform.IntegrationTests;

public sealed class PlatformWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(
            "ConnectionStrings:Default",
            "Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform");
        builder.UseSetting("Platform:AccessKey", "integration-test-access-key");
        builder.UseSetting("Platform:CookieSecure", "false");
        builder.UseSetting("Platform:PublicHealth", "false");
        builder.UseSetting("MemoryWorker:ServiceToken", "integration-memory-worker-token");
        builder.UseSetting("MemoryWorker:PrimaryUserId", "1");
    }
}
