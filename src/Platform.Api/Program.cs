using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Platform.Api.Access;
using Platform.Application.Configuration;
using Platform.Application.Abstractions;
using Platform.Api.Features;
using Platform.Api.Features.Access;
using Platform.Api.Middleware;
using Platform.Application;
using Platform.Application.Features.Memory.Exceptions;
using Platform.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
LoadDevelopmentEnvFile(builder);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOptions<PlatformAccessOptions>()
    .Bind(builder.Configuration.GetSection(PlatformAccessOptions.SectionName))
    .Validate(o => o.SessionHours is >= 1 and <= 168, "SessionHours must be between 1 and 168.")
    .ValidateOnStart();

builder.Services.AddSingleton<PlatformAccessSessionService>();

builder.Services.AddOptions<PlatformWorkerOptions>()
    .Bind(builder.Configuration.GetSection(PlatformWorkerOptions.SectionName));

var dataProtectionKeysPath = builder.Configuration["Platform:DataProtectionKeysPath"];
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("Platform");
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    var keysDirectory = new DirectoryInfo(dataProtectionKeysPath);
    keysDirectory.Create();
    dataProtection.PersistKeysToFileSystem(keysDirectory);
}

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

// Auto-migrate only in Development and Testing — production migrations are run explicitly.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>();
    await db.MigrateAsync().ConfigureAwait(false);
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("Platform.Errors");

        // B1: FluentValidation → 400 ProblemDetails
        if (feature?.Error is ValidationException vex)
        {
            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await Results.ValidationProblem(errors)
                .ExecuteAsync(context)
                .ConfigureAwait(false);
            return;
        }

        // C4: MemoryApplicationException → 409 Conflict.
        if (feature?.Error is MemoryApplicationException mex)
        {
            await Results.Problem(
                    title: "Conflict",
                    detail: mex.Message,
                    statusCode: StatusCodes.Status409Conflict)
                .ExecuteAsync(context)
                .ConfigureAwait(false);
            return;
        }

        if (feature?.Error is not null)
        {
            logger.LogError(feature.Error, "Unhandled exception");
        }

        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (env.IsEnvironment("Testing") && feature?.Error is Exception testingEx)
        {
            await Results.Problem(
                    title: "Unhandled exception (testing)",
                    detail: testingEx.ToString(),
                    statusCode: StatusCodes.Status500InternalServerError)
                .ExecuteAsync(context)
                .ConfigureAwait(false);
            return;
        }

        await Results.Problem("An unexpected error occurred.").ExecuteAsync(context).ConfigureAwait(false);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HttpsRedirection needs ASPNETCORE_HTTPS_PORT (or an https URL in launchSettings). The "http"
// launch profile is HTTP-only, which triggers "Failed to determine the https port" at startup.
if (!app.Environment.IsEnvironment("Testing"))
{
    var devHttpOnly =
        app.Environment.IsDevelopment()
        && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_HTTPS_PORT"));
    if (!devHttpOnly)
    {
        app.UseHttpsRedirection();
    }
}
app.UseCors("platform");
app.UseRateLimiter();
app.UseMiddleware<InternalWorkerAuthenticationMiddleware>();
app.UseMiddleware<RequirePlatformAccessMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// C3: /ready uses IDatabaseHealthCheck — no direct EF reference in the Api host.
app.MapGet(
    "/ready",
    async (IDatabaseHealthCheck health, CancellationToken ct) =>
    {
        var canConnect = await health.CanConnectAsync(ct).ConfigureAwait(false);
        return canConnect ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
    });

app.MapAdminEndpoints();
app.MapV1Endpoints();
app.MapInternalEndpoints();

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
