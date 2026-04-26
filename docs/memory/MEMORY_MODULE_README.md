# Memory module — layout in `backend-platform`

This describes the **Memory** bounded context after the **structure-only** pass (no EF mapping for new entities yet, no pgvector, no workers). It aligns with [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) and [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md).

## Principles

- **Application** defines ports and use cases; **Infrastructure** implements them; **Api** is HTTP only.
- **New** memory tables are **not** in `PlatformDbContext` until a migration is added. Placeholder domain types exist for the schema you will map next.
- The pre-existing **`MemoryInsights`** table and `GET /api/v1/memory/insights` remain for the current UI; the entity file lives under **Domain `Legacy/`** and is read via `ILegacyMemoryInsightsReadRepository`.

## Layer map

| Layer | Path (under `src/`) | Role |
| --- | --- | --- |
| **Domain** | `Platform.Domain/Features/Memory/` | Entities, enums, value objects. Subfolders: `Entities/`, `Enums/`, `ValueObjects/`, `Legacy/`. |
| **Contracts** | `Platform.Contracts/V1/Memory/` | Wire DTOs for v1 (shell + future list DTOs). `MemoryInsightDto` stays in `V1/` root for legacy. |
| **Application — abstractions** | `Platform.Application/Abstractions/Memory/` | Ports grouped by area: `Events/`, `Items/`, `Semantic/`, `Procedural/`, `Review/`, `Context/`, `Legacy/`. |
| **Application — features** | `Platform.Application/Features/Memory/` | Use cases: `Events/IngestEvent/`, `Items/ListItems/`, `Context/…`, `Profile/…`, `Semantic/…`, `Procedural/…`, `ReviewQueue/` (list, create, approve, reject, patch), `Legacy/Insights/`, plus `DependencyInjection/`. |
| **Infrastructure** | `Platform.Infrastructure/Features/Memory/` | `Context/` (`EfMemoryContextProvider`), `Events/`, `Profile/`, `Semantic/` (`EfSemanticMemoryService`, `EfSemanticMemoryReadRepository`), `Procedural/` (`EfProceduralRuleService`), `Review/`, `Legacy/`, `Stubs/`, `DependencyInjection/`. |
| **Api** | `Platform.Api/Features/Memory/` | `MemoryV1Routes` composes `Module/`, `Context/`, `Events/`, `Profile/`, `Semantic/`, `Procedural/`, and `Legacy/Insights/`. |

## Dependency registration

- **Application:** `AddMemoryApplication()` in `MemoryApplicationServiceCollectionExtensions.cs` — called from `Platform.Application/DependencyInjection.cs`.
- **Infrastructure:** `AddPlatformMemoryInfrastructure(IConfiguration)` in `MemoryInfrastructureServiceCollectionExtensions.cs` — called at the start of `Platform.Infrastructure/DependencyInjection.cs` (stubs + legacy + vector ports + future ports).

## HTTP (v1)

- `GET /api/v1/memory/structure` — `MemoryModuleDescriptorV1Dto` (module version + area names). No DB.
- `GET/PUT /api/v1/memory/explicit-profile` — `ProfileMemoryV1Dto` / `UpdateProfileMemoryV1Request` (typed profile row; see [05-profile-memory.md](05-profile-memory.md)).
- `GET/POST/… /api/v1/memory/semantics` — learned claims + evidence management (see [08-semantic-memory.md](08-semantic-memory.md)).
- `GET/POST/… /api/v1/memory/procedural-rules` — versioned behavior rules (see [11-procedural-memory.md](11-procedural-memory.md)).
- `POST /api/internal/v1/memory/consolidation/nightly` — **Bearer**-authenticated consolidation (see [10-nightly-worker.md](10-nightly-worker.md)); not for browser clients.
- `POST /api/v1/memory/context` — `GetMemoryContextV1Request` → `MemoryContextV1Dto` (curated packet; see [06-retrieval-engine.md](06-retrieval-engine.md)).
- `POST /api/v1/memory/embeddings/upsert` — governed embedding refresh for a `memory_items` row (see [12-vector-memory.md](12-vector-memory.md)).
- `GET/POST/PATCH …/memory/review-queue` — review proposals (see [07-review-queue.md](07-review-queue.md)).
- `POST /api/v1/memory/events` — memory event ingest (episodic append).
- `GET /api/v1/memory/insights` — legacy list from `MemoryInsights` (unchanged path for the frontend until removal).

Some handlers (e.g. list items, review queue, get context shell) are **registered in DI** but **not** all exposed on HTTP; see the Api feature folders for the current route map.

## See also

- [02-db-schema.md](02-db-schema.md) — governed memory **tables v1** (migration `MemorySystemV1`), indexes, and column reference.
- [03-domain-model.md](03-domain-model.md) — **domain behavior** and **application ports** (`IMemoryContextProvider`, `IExplicitUserProfileRepository`, review/semantic services, `IMemoryEventWriter`).
- [05-profile-memory.md](05-profile-memory.md) — explicit `memory_explicit_profile` (GET/PUT `/api/v1/memory/explicit-profile`).
- [06-retrieval-engine.md](06-retrieval-engine.md) — GetMemoryContext v1 (POST `/api/v1/memory/context`).
- [07-review-queue.md](07-review-queue.md) — review queue and approval workflow.
- [11-procedural-memory.md](11-procedural-memory.md) — procedural rules v1 (CRUD, review, context).
- [12-vector-memory.md](12-vector-memory.md) — pgvector embeddings and recall.
- [LEGACY_MEMORY_REMOVAL.md](LEGACY_MEMORY_REMOVAL.md) — full inventory of legacy `MemoryInsight` / `GET memory/insights` to delete in one pass when ready.

## Next steps (out of scope for the structure pass)

- EF: `DbSet`s, `OnModelCreating`, and **migrations** for new tables.
- Replace stubs with real repositories; wire optional internal/API routes and validation.
- **Vector search** and **workers** per roadmap.
