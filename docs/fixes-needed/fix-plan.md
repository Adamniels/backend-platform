# Architecture and standards fix plan

**Source:** [architecture-and-standards-audit.md](./architecture-and-standards-audit.md)  
**Target framework:** net10.0 throughout — all `Microsoft.Extensions.*` and EF packages on `10.x`.  
**Convention:** Items are ordered by implementation dependency (things that later items build on come first), not just severity.

---

## Group A — Quick wins (low risk, self-contained)

These touch one or two files each and carry no cross-layer ripple.

### A1 — Align package versions to net10 (audit §7)

All `Microsoft.Extensions.*` packages in `Platform.Application` are currently on `9.0.0`. Everything else is already on `10.0.0`.

**Files to change:**

- `Platform.Application/Platform.Application.csproj` — bump all `Microsoft.Extensions.*` references from `9.0.0` → `10.0.0`

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions"  Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Options"               Version="10.0.0" />
```

**Done when:** `dotnet build` succeeds with no version warnings.

---

### A2 — Remove default value from `NewsItemSummaryDto.RelevanceScore` (audit §2.5)

Optional parameters in positional records used inside EF `Select` projections cause CS0854. The convention going forward is: if a DTO is projected in EF, all constructor arguments must be explicit.

**Files to change:**

- `Platform.Contracts/V1/NewsItemSummaryDto.cs` — keep the parameter, remove the default:

```csharp
public sealed record NewsItemSummaryDto(
    string  Id,
    string  Title,
    string  Source,
    string  PublishedAt,
    string? Url,
    string? Body,
    double? RelevanceScore);
```

- `Platform.Infrastructure/Features/News/NewsReadRepository.cs` — already passes explicit `null`; no change needed.
- Any other site that constructs `NewsItemSummaryDto` without providing all seven arguments will get a compile error, which is the desired safety net.

**Document in** `docs/infrastructure-guide.md`: "DTOs projected in EF `Select` must not use optional/default constructor arguments."

**Done when:** `dotnet build` succeeds with no CS0854 warnings or errors.

---

### A3 — Reword `IProceduralRuleService` XML comments (audit §2.7)

The comment on `IProceduralRuleService` references DbContext and transactions — implementation details that should not appear in a port.

**File to change:**

- `Platform.Application/Abstractions/Memory/Procedural/IProceduralRuleService.cs`

Replace any reference to `DbContext`, `SaveChangesAsync`, or transaction terminology with "within the same unit of work" or simply omit the implementation note entirely.

**Done when:** No persistence-layer terminology remains in Application port XML docs.

---

### A4 — Deduplicate workflow status formatting (audit §2.6)

`WorkflowRunStatusFormatter` (Application) and `WorkflowRunStatusMapper` (Infrastructure) are identical 17-line classes.

**Plan:**

1. Keep `WorkflowRunStatusFormatter` in `Platform.Application/Features/WorkflowRuns/Shared/`.
2. Update `WorkflowRunRepository` in Infrastructure to import and call `WorkflowRunStatusFormatter` instead of `WorkflowRunStatusMapper`.
3. Delete `WorkflowRunStatusMapper.cs`.

**Files to change:**

- `Platform.Infrastructure/Features/WorkflowRuns/WorkflowRunRepository.cs` — replace `WorkflowRunStatusMapper.Map(...)` call with `WorkflowRunStatusFormatter.Format(...)`
- `Platform.Infrastructure/Features/WorkflowRuns/WorkflowRunStatusMapper.cs` — delete

**Done when:** `dotnet build` succeeds; `WorkflowRunStatusMapper` no longer exists.

---

### A5 — Consolidate internal route registration (audit §3.1)

Internal routes are mapped directly in `Program.cs` after `MapV1Endpoints()`. Add a `MapInternalEndpoints()` extension so the host only calls two methods.

**Files to change:**

- Create `Platform.Api/Features/InternalApiRegistration.cs`:

```csharp
public static class InternalApiRegistration
{
    public static void MapInternalEndpoints(this WebApplication app)
    {
        InternalMemoryV1Routes.Map(app);
        InternalNewsV1Routes.Map(app);
        InternalSideLearningV1Routes.Map(app);
    }
}
```

- `Platform.Api/Program.cs` — replace the three individual `Map(app)` calls with `app.MapInternalEndpoints()`.

**Done when:** `dotnet build` succeeds; `Program.cs` has a single internal-routes call.

---

### A6 — Remove legacy memory insights (audit §4.2)

`LEGACY_MEMORY_REMOVAL.md` documents these as pending removal.

**Files to change/delete:**

- `Platform.Api/Features/Memory/Legacy/Insights/MemoryInsightsV1Routes.cs` — delete
- `Platform.Application/Features/Memory/` — find and delete `ListMemoryInsightsQueryHandler` and any associated query/DTO
- `Platform.Application/DependencyInjection.cs` — remove `ListMemoryInsightsQueryHandler` registration
- `Platform.Infrastructure/` — find and delete `ILegacyMemoryInsightsReadRepository` and its EF implementation; remove registration from memory DI extensions
- `Platform.Api/Features/InternalApiRegistration.cs` (or V1ApiRegistration.cs) — remove route mapping

Verify no other file imports these types after deletion.

**Done when:** `dotnet build` succeeds; no `Insight`-related legacy types remain in source.

---

## Group B — Validation and error handling

### B1 — FluentValidation → 400 ProblemDetails in the exception handler (audit §2.3)

**File to change:**

- `Platform.Api/Program.cs` — add a branch in the global exception handler before the generic 500 path:

```csharp
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
```

Add `using FluentValidation;` to `Program.cs`.

**Done when:** A request that fails FluentValidation returns HTTP 400 with RFC 7807 field errors, not 500.

---

### B2 — Move `PublishedAt` parsing into FluentValidation (audit §2.4)

Currently `InternalNewsV1Routes.cs` parses `PublishedAt` and returns 400 from the route. The command should accept the raw string; FluentValidation validates and the handler parses it after validation passes.

**Files to change:**

- `Platform.Contracts/V1/News/IngestNewsItemV1Contracts.cs` — `IngestNewsItemV1Request.PublishedAt` is already a `string`; no change needed.
- `Platform.Application/Features/News/Ingest/IngestNewsItemCommand.cs` — change `PublishedAt` from `DateTimeOffset` to `string`.
- `Platform.Application/Features/News/Ingest/IngestNewsItemCommandValidator.cs` — add rule:

```csharp
RuleFor(x => x.PublishedAt)
    .NotEmpty()
    .Must(s => DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture,
                   DateTimeStyles.RoundtripKind, out _))
    .WithMessage("Expected an ISO-8601 date/time string.");
```

- `Platform.Application/Features/News/Ingest/IngestNewsItemCommandHandler.cs` — parse `PublishedAt` after validation (it is now guaranteed valid).
- `Platform.Api/Features/News/Internal/InternalNewsV1Routes.cs` — remove the `TryParse` block; pass raw string directly into the command.

**Done when:** Route is thin; invalid `publishedAt` returns 400 via FluentValidation pipeline.

---

### B3 — Add validators for new Phase 2 handlers (audit §6)

**Files to create:**

- `Platform.Application/Features/News/Embed/EmbedNewsItemCommandValidator.cs`:

```csharp
public sealed class EmbedNewsItemCommandValidator : AbstractValidator<EmbedNewsItemCommand>
{
    public EmbedNewsItemCommandValidator()
    {
        RuleFor(x => x.NewsItemId)
            .NotEmpty()
            .MaximumLength(64)
            .Matches(@"^ni-[0-9a-f]{32}$")
            .WithMessage("NewsItemId must be a valid ni- prefixed identifier.");
    }
}
```

- `Platform.Application/Features/News/Profile/SeedNewsProfileCommandValidator.cs`:

```csharp
public sealed class SeedNewsProfileCommandValidator : AbstractValidator<SeedNewsProfileCommand>
{
    public SeedNewsProfileCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
    }
}
```

Both validators are auto-discovered by `AddValidatorsFromAssembly` — no DI change needed.

**Done when:** `dotnet build` succeeds; invalid inputs return 400 via the FluentValidation pipeline (once B1 is done).

---

## Group C — Layering fixes

### C1 — Fix `IMemoryEmbeddingGenerator` lifetime mismatch (audit §2.8)

Memory infrastructure registers the deterministic stub as **Singleton**. Phase 2 work then registers `OpenAiEmbeddingGenerator` as **Scoped**. Last registration wins, which is correct functionally, but the stub stays in the container as a dead singleton and the lifetime is inconsistent.

**Files to change:**

- `Platform.Infrastructure/Features/Memory/DependencyInjection/MemoryInfrastructureServiceCollectionExtensions.cs` — remove the `IMemoryEmbeddingGenerator` registration from this file entirely. The memory DI extension should no longer own this registration.
- `Platform.Infrastructure/DependencyInjection.cs` — `OpenAiEmbeddingGenerator` registration stays as-is but change from `AddScoped` to `AddSingleton`. The generator holds no request-scoped state (it is stateless HTTP calls) and `IHttpClientFactory` is safe for singleton use.

**Done when:** Only one registration of `IMemoryEmbeddingGenerator` exists; it is `AddSingleton<IMemoryEmbeddingGenerator, OpenAiEmbeddingGenerator>`.

---

### C2 — Remove `Pgvector.Vector` from Domain and Application (audit §1.3)

Domain entities currently expose `Pgvector.Vector` as a property type, and Application handlers call `new Vector(float[])`. The correct boundary is: Domain and Application work with `float[]`; Infrastructure maps to `Vector` when writing to EF and back to `float[]` when reading.

**Domain changes:**

- `Platform.Domain/Features/News/NewsItemEmbedding.cs` — change `public Vector Embedding` → `public float[] Embedding`
- `Platform.Domain/Features/News/NewsUserProfile.cs` — change `public Vector LongTermEmbedding` → `public float[] LongTermEmbedding`
- `Platform.Domain/Features/Memory/Entities/MemoryEmbedding.cs` — same treatment for `public Vector Embedding`
- Remove `<PackageReference Include="Pgvector" ...>` from `Platform.Domain.csproj`

**Application changes:**

- `Platform.Application/Features/News/Embed/EmbedNewsItemCommandHandler.cs` — remove `using Pgvector;`; assign `Embedding = vector` (raw `float[]`) instead of `new Vector(vector)`.
- `Platform.Application/Features/News/Profile/SeedNewsProfileCommandHandler.cs` — same; assign `LongTermEmbedding = vector` directly.
- Remove `<PackageReference Include="Pgvector" ...>` from `Platform.Application.csproj`

**Infrastructure changes (where Vector is constructed):**

- `Platform.Infrastructure/Persistence/PlatformDbContext.cs` — EF configuration for embedding columns stays as `HasColumnType("vector(1536)")`; EF Core + Pgvector handles the CLR↔pgvector mapping via the `HasColumnType` annotation and the `.UseVector()` call on the options — no property-level `Vector` type is needed on the entity.

  Actually, the Pgvector EF provider maps `float[]` properties to `vector` columns when `HasColumnType("vector(N)")` is configured. Verify this works with `Pgvector.EntityFrameworkCore` 0.2.x — if it does not, use a backing field with `[NotMapped]` and expose `float[]` from Domain while storing `Vector` in a private Infrastructure-only EF shadow property.

- `Platform.Infrastructure/Features/News/EfNewsEmbeddingRepository.cs` — if `Vector` construction is needed for writes, do it here: `new Vector(embedding.Embedding)`.
- `Platform.Infrastructure/Features/News/NewsVectorSearch.cs` — `CosineDistance` extension takes a `Vector`; construct from the profile's `float[]` inline: `new Vector(profile.LongTermEmbedding)`.
- `Platform.Infrastructure/Persistence/MemoryV1EfConfiguration.cs` — same treatment for `MemoryEmbedding`.
- `Platform.Infrastructure` keeps `<PackageReference Include="Pgvector">` and `<PackageReference Include="Pgvector.EntityFrameworkCore">`.

**Done when:** `Platform.Domain.csproj` and `Platform.Application.csproj` have no Pgvector references; `dotnet build` succeeds.

---

### C3 — Remove `PlatformDbContext` from Api host (audit §1.1 and §2.1)

**Step 1 — Introduce a readiness port in Application:**

```csharp
// Platform.Application/Abstractions/IDatabaseHealthCheck.cs
public interface IDatabaseHealthCheck
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
```

**Step 2 — Implement in Infrastructure:**

```csharp
// Platform.Infrastructure/Health/EfDatabaseHealthCheck.cs
public sealed class EfDatabaseHealthCheck(PlatformDbContext db) : IDatabaseHealthCheck
{
    public Task<bool> CanConnectAsync(CancellationToken ct) =>
        db.Database.CanConnectAsync(ct);
}
```

Register in `DependencyInjection.cs`: `services.AddScoped<IDatabaseHealthCheck, EfDatabaseHealthCheck>()`.

**Step 3 — Update `Program.cs`:**

- Remove `using Platform.Infrastructure.Persistence;`
- Replace `PlatformDbContext` in `/ready` route with `IDatabaseHealthCheck` injected from DI
- Gate migrations on environment:

```csharp
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    await db.Database.MigrateAsync().ConfigureAwait(false);
}
```

  Note: this leaves `PlatformDbContext` referenced in `Program.cs` still, but only in a dev/test guard. For full removal, extract migration into a hosted service or CLI tool (document as future work in `docs/backend-standards.md`).

**Done when:** `/ready` route has no direct EF reference; startup migration is guarded by environment.

---

### C4 — Map `MemoryConflictException` via Application outcome (audit §1.2)

**Step 1 — Application outcome type:**

```csharp
// Platform.Application/Features/Memory/Exceptions/MemoryApplicationException.cs
public sealed class MemoryApplicationException : Exception
{
    public MemoryApplicationException(string message) : base(message) { }
}
```

**Step 2 — In handlers that currently throw `MemoryConflictException`:** catch the domain exception and rethrow as `MemoryApplicationException` (or return a result type — preferred but larger change).

**Step 3 — `Program.cs`:** catch `MemoryApplicationException` instead of `MemoryConflictException`; remove `using Platform.Domain.Features.Memory`.

**Step 4 — `InternalMemoryV1Routes.cs`:** remove the per-route `catch (MemoryConflictException)` — the middleware handles it.

**Done when:** No `using Platform.Domain` appears in `Platform.Api` source files.

---

### C5 — Implement `IMemoryItemReadRepository` (audit §4.1)

Replace `MemoryItemReadRepositoryStub` (which returns empty list) with a real EF implementation.

**File to create:**

- `Platform.Infrastructure/Features/Memory/Items/EfMemoryItemReadRepository.cs`

Implement `ListSummariesForUserAsync(int userId, CancellationToken ct)` using `PlatformDbContext.MemoryItems`.

**File to update:**

- `MemoryInfrastructureServiceCollectionExtensions.cs` — replace `MemoryItemReadRepositoryStub` registration with `EfMemoryItemReadRepository`.

**File to delete:**

- `Platform.Infrastructure/Features/Memory/Stubs/EmptyListMemoryStubs.cs` (or remove only the `MemoryItemReadRepositoryStub` class if `ProceduralRuleReadRepositoryStub` is still needed).

**Done when:** `IMemoryItemReadRepository` returns real data; stub is removed.

---

### C6 — Fail fast on missing connection string outside Development (audit §2.2)

**File to change:**

- `Platform.Infrastructure/DependencyInjection.cs`:

```csharp
var connectionString = configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    var env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
    var isDev = string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase)
             || string.Equals(env, "Testing",     StringComparison.OrdinalIgnoreCase);

    if (!isDev)
        throw new InvalidOperationException(
            "ConnectionStrings:Default is required in non-Development environments.");

    connectionString = "Host=localhost;Port=5432;Database=platform;Username=platform;Password=platform";
}
```

**Done when:** Starting the API without a connection string in a non-dev environment throws a clear startup error rather than silently using the hardcoded fallback.

---

### C7 — Hardcoded `UserId: 1` on public news feed (audit §3.2)

This is a single-user system today but the pattern should be explicit.

**Short-term (this fix):**

- `Platform.Api/Program.cs` already has `PlatformWorkers:PrimaryUserId = 1` in `appsettings.json` via `PlatformWorkerOptions`.
- Add the primary user ID to `PlatformWorkerOptions` or a new `PlatformOptions` type and inject it via `IOptions<>` into the route.

**File to change:**

- `Platform.Api/Features/News/NewsV1Routes.cs` — inject `IOptions<PlatformWorkerOptions>` and use `options.Value.PrimaryUserId` instead of the literal `1`.

**Done when:** The user ID is driven by config, not a hardcoded literal.

---

## Group D — Testing

### D1 — News feed vector ranking unit tests

**Files to create** (`Platform.Tests.Unit` or equivalent):

- `NewsListFeedQueryHandlerTests.cs`
  - When vector search returns hits: result is relevance-ordered with `RelevanceScore` populated.
  - When vector search returns empty: result falls back to `ListFeedAsync` (chronological).
  - Mock `INewsVectorSearch` and `INewsReadRepository`.

### D2 — News embed / profile seed unit tests

- `EmbedNewsItemCommandHandlerTests.cs`
  - Returns `Embedded` when body exists and embedding succeeds.
  - Returns `Skipped` when embedding already exists.
  - Returns `Error` when body is null or generator returns null.
- `SeedNewsProfileCommandHandlerTests.cs`
  - Returns `Seeded` on first call.
  - Returns `Exists` when profile already present.
  - Returns `Error` when seed text is empty or generator returns null.

### D3 — News embed / profile seed HTTP integration tests

Following the pattern of `InternalNewsV1Tests`:

- `InternalNewsEmbedV1Tests.cs` — `POST /api/internal/v1/news/items/{id}/embed` returns `embedded`/`skipped`/`error` for valid, duplicate, and missing IDs.
- `InternalNewsProfileV1Tests.cs` — `POST /api/internal/v1/news/profile/seed` returns `seeded` on first call, `exists` on repeat.

---

## Implementation order

```
A1  →  A2  →  A3  →  A4  →  A5  →  A6
B1  →  B2  →  B3
C1  →  C2  →  C3  →  C4  →  C5  →  C6  →  C7
D1  →  D2  →  D3
```

Groups A and B are independent of each other and of C/D.  
C2 (Pgvector removal) should be done before C3 and C4 as it clarifies what `Platform.Domain` exports.  
D tests can be written alongside or after B validators but benefit from C fixes being in place.

---

## Files changed or created — full index

| File | Action | Group |
| --- | --- | --- |
| `Platform.Application/Platform.Application.csproj` | Bump Extensions to 10.x; remove Pgvector | A1, C2 |
| `Platform.Domain/Platform.Domain.csproj` | Remove Pgvector | C2 |
| `Platform.Contracts/V1/NewsItemSummaryDto.cs` | Remove default from RelevanceScore | A2 |
| `Platform.Application/Abstractions/Memory/Procedural/IProceduralRuleService.cs` | Reword XML comments | A3 |
| `Platform.Infrastructure/Features/WorkflowRuns/WorkflowRunStatusMapper.cs` | Delete | A4 |
| `Platform.Infrastructure/Features/WorkflowRuns/WorkflowRunRepository.cs` | Use WorkflowRunStatusFormatter | A4 |
| `Platform.Api/Features/InternalApiRegistration.cs` | Create | A5 |
| `Platform.Api/Program.cs` | Use MapInternalEndpoints, gate migrations, remove DbContext from /ready, add ValidationException handler | A5, B1, C3 |
| Legacy insights files (routes, handler, repo, DI) | Delete | A6 |
| `Platform.Application/Features/News/Ingest/IngestNewsItemCommand.cs` | PublishedAt → string | B2 |
| `Platform.Application/Features/News/Ingest/IngestNewsItemCommandValidator.cs` | Add PublishedAt rule | B2 |
| `Platform.Application/Features/News/Ingest/IngestNewsItemCommandHandler.cs` | Parse PublishedAt after validation | B2 |
| `Platform.Api/Features/News/Internal/InternalNewsV1Routes.cs` | Remove TryParse, thin route | B2 |
| `Platform.Application/Features/News/Embed/EmbedNewsItemCommandValidator.cs` | Create | B3 |
| `Platform.Application/Features/News/Profile/SeedNewsProfileCommandValidator.cs` | Create | B3 |
| `Platform.Infrastructure/Features/Memory/DependencyInjection/MemoryInfrastructureServiceCollectionExtensions.cs` | Remove IMemoryEmbeddingGenerator registration | C1 |
| `Platform.Infrastructure/DependencyInjection.cs` | Change OpenAiEmbeddingGenerator to Singleton; fail-fast on missing conn string | C1, C6 |
| `Platform.Domain/Features/News/NewsItemEmbedding.cs` | float[] Embedding | C2 |
| `Platform.Domain/Features/News/NewsUserProfile.cs` | float[] LongTermEmbedding | C2 |
| `Platform.Domain/Features/Memory/Entities/MemoryEmbedding.cs` | float[] Embedding | C2 |
| `Platform.Application/Features/News/Embed/EmbedNewsItemCommandHandler.cs` | Remove new Vector(); assign float[] | C2 |
| `Platform.Application/Features/News/Profile/SeedNewsProfileCommandHandler.cs` | Remove new Vector(); assign float[] | C2 |
| `Platform.Infrastructure/Features/News/EfNewsEmbeddingRepository.cs` | Construct Vector here if needed | C2 |
| `Platform.Infrastructure/Features/News/NewsVectorSearch.cs` | Construct Vector from float[] | C2 |
| `Platform.Infrastructure/Persistence/MemoryV1EfConfiguration.cs` | Verify float[] maps to vector column | C2 |
| `Platform.Application/Abstractions/IDatabaseHealthCheck.cs` | Create | C3 |
| `Platform.Infrastructure/Health/EfDatabaseHealthCheck.cs` | Create | C3 |
| `Platform.Application/Features/Memory/Exceptions/MemoryApplicationException.cs` | Create | C4 |
| Memory handlers that throw MemoryConflictException | Wrap in MemoryApplicationException | C4 |
| `Platform.Api/Features/Memory/Internal/InternalMemoryV1Routes.cs` | Remove per-route catch | C4 |
| `Platform.Infrastructure/Features/Memory/Items/EfMemoryItemReadRepository.cs` | Create | C5 |
| `Platform.Infrastructure/Features/Memory/Stubs/EmptyListMemoryStubs.cs` | Remove stub or delete file | C5 |
| `Platform.Api/Features/News/NewsV1Routes.cs` | UserId from config | C7 |
| Test files (unit + integration for news feed, embed, seed) | Create | D1–D3 |
