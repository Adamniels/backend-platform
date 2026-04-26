using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Platform.Api.Access;
using Platform.Application.Configuration;
using Platform.Api.Features;
using Platform.Api.Features.Access;
using Platform.Api.Middleware;
using Platform.Application;
using Platform.Infrastructure;
using Platform.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
LoadDevelopmentEnvFile(builder);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOptions<PlatformAccessOptions>()
    .Bind(builder.Configuration.GetSection(PlatformAccessOptions.SectionName))
    .Validate(o => o.SessionHours is >= 1 and <= 168, "SessionHours must be between 1 and 168.")
    .ValidateOnStart();

builder.Services.AddSingleton<PlatformAccessSessionService>();

builder.Services.AddDataProtection();
// Optional: persist Data Protection key ring across restarts by setting Platform:DataProtectionKeysPath
// and configuring PersistKeysToFileSystem (requires key ring package aligned with your SDK).

builder.Services.AddProblemDetails();
builder.Services.AddRateLimiter(o => o.AddUnlockRateLimiter());

builder.Services.AddCors(o =>
{
    o.AddPolicy(
        "platform",
        p =>
        {
            var origins = builder.Configuration.GetSection(PlatformAccessOptions.SectionName).Get<PlatformAccessOptions>()?.AllowedOrigins;
            if (origins is { Length: > 0 })
            {
                p.WithOrigins(origins);
            }
            else
            {
                p.WithOrigins("http://localhost:3000", "https://localhost:3000");
            }

            p.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddPlatformApplication();
builder.Services.AddPlatformInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await db.Database.MigrateAsync().ConfigureAwait(false);
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Platform.Errors");
        if (feature?.Error is not null)
        {
            logger.LogError(feature.Error, "Unhandled exception");
        }

        await Results.Problem("An unexpected error occurred.").ExecuteAsync(context).ConfigureAwait(false);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
app.UseCors("platform");
app.UseRateLimiter();
app.UseMiddleware<RequirePlatformAccessMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet(
    "/ready",
    async (PlatformDbContext db, CancellationToken ct) =>
    {
        var canConnect = await db.Database.CanConnectAsync(ct).ConfigureAwait(false);
        return canConnect ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
    });

app.MapAdminEndpoints();
app.MapV1Endpoints();

app.Run();

static void LoadDevelopmentEnvFile(WebApplicationBuilder builder)
{
    if (!builder.Environment.IsDevelopment())
    {
        return;
    }

    var candidates = new[]
    {
        Path.Combine(builder.Environment.ContentRootPath, ".env"),
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".env")),
    };

    var envPath = candidates.FirstOrDefault(File.Exists);
    if (envPath is null)
    {
        return;
    }

    foreach (var rawLine in File.ReadLines(envPath))
    {
        var line = rawLine.Trim();
        if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
        {
            continue;
        }

        if (line.StartsWith("export ", StringComparison.OrdinalIgnoreCase))
        {
            line = line[7..].Trim();
        }

        var separator = line.IndexOf('=');
        if (separator <= 0)
        {
            continue;
        }

        var key = line[..separator].Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        if (Environment.GetEnvironmentVariable(key) is not null)
        {
            continue; // honor explicit shell/env overrides
        }

        var value = line[(separator + 1)..].Trim();
        if (value.Length >= 2 &&
            ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\''))))
        {
            value = value[1..^1];
        }

        Environment.SetEnvironmentVariable(key, value);
    }
}

public partial class Program;
