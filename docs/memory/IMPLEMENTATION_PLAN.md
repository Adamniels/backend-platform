# Memory system — implementation plan (repository-aligned)

This document turns [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) into a phased plan that **fits the current `Platform` repo** (`backend-platform`, `workers-platform`). Open items are listed under [Decisions and questions](#decisions-and-questions) unless superseded by [Locked product decisions](#locked-product-decisions) below.

---

## Locked product decisions

Recorded from stakeholder input (2026-04-26). These drive Phase 1 implementation.

| # | Topic | Decision |
| --- | --- | --- |
| 1 | **Tenancy** | **Single-tenant** — one logical user for the product. No multi-user identity in scope for this memory rollout. |
| 2 | **Profile + memory shape** | **New normalized schema** for explicit profile and the memory “truth layer” at the **center** of the product — not a thin add-on to the first-draft singleton mock. Favor clear relational tables (profile facts, memory domain tables) over stuffing JSON into a legacy `PlatformProfile` row. The old `PlatformProfile` / settings rows can be **replaced, migrated, or demoted** as part of the same effort so there is a single coherent model (exact mapping is an implementation choice in the first PR). |
| 3 | **Current draftcode** | **Remove / replace** first-pass artifacts: `MemoryInsight` entity, seed `HasData`, `GET /api/v1/memory/insights`, `MemoryInsightDto`, and related handler/port/repo. Treat the codebase as a clean baseline aligned to the master doc’s tables and flows. **Search/replace** any UI or contract consumers in-repo when you open the first implementation prompt. |
| 4 | **GetMemoryContext (HTTP)** | **Later phase** — do not block schema, ingestion, and internal use cases on the public DTO/endpoint. Internal handlers or a narrow internal API can exist before the final agent-facing `GetMemoryContext` contract. |
| 5 | **Event writers + worker access** | See [Recommendation: events ingest and workers](#recommendation-events-ingest-and-workers) (agreed as direction unless you override in a phase prompt). |

### Recommendation: events ingest and workers

For **single-tenant** and **one system of record in PostgreSQL**:

- **Principle:** The **.NET service owns all writes** to memory tables (same pattern as the rest of `Platform`: truth in the API/EF layer, workers are orchestration and compute, not a second database owner).
- **`memory_events` in early phases:** Prefer the **.NET API** as the only writer: UI and server-side use cases go through normal authenticated routes. **Python workers** should **not** be given the Postgres connection string for memory tables in steady state; they should call an **ingestion API** on the platform when a workflow has evidence to record.
- **How workers authenticate to the API:** The **browser cookie** does not apply to headless workers. A practical pattern for a personal single-tenant stack:
  - **Header-based API key** (e.g. `X-Internal-Key` or `Authorization: …`) for **ingestion-only** routes, configured via user secrets / env, validated in a small **Api**-layer middleware or `IAuthorizationHandler` scoped to those paths.
  - Keep **antiforgery** disabled on those JSON posts (same as existing `api-guidelines` patterns) and **rate-limit** in production.
  - If you outgrow this, evolve to mTLS or OAuth client credentials in a later hardening pass — not required to start.

This keeps **one write path** through your domain rules, makes audits straightforward, and matches the master doc’s “platform owns memory.”

You can refine key rotation and per-route scopes when you have more than one worker class.

---

## 1. Current backend architecture

| Area | What exists today |
| --- | --- |
| **Style** | **Modular monolith**: Clean Architecture–style layers, **no MediatR** — concrete `*QueryHandler` / `*CommandHandler` types registered in DI and called from **minimal API** route delegates. |
| **Projects** | `Platform.Api` → `Platform.Application` + `Platform.Infrastructure` + `Platform.Contracts`; `Platform.Application` → `Platform.Domain` + `Platform.Contracts`; `Platform.Infrastructure` → `Platform.Application` + `Platform.Domain`. |
| **HTTP** | **ASP.NET Core minimal APIs** (no controllers). Versioned product API: `MapGroup("/api/v1")` in `backend-platform/src/Platform.Api/Features/V1ApiRegistration.cs`. |
| **Docs** | Normative: `backend-platform/docs/backend-standards.md`, `architecture.md`, `api-guidelines.md`, `feature-development-guide.md`, `naming-conventions.md`, `persistence-guide.md`, `auth-and-security.md`. |

**Implication for memory:** New endpoints, commands/queries, ports, and EF entities should follow the same checklist as any other feature (Contracts → Domain + migration → port + infra + handler + routes + tests).

---

## 2. Existing database and migration setup

| Topic | Detail |
| --- | --- |
| **ORM** | **EF Core** with **Npgsql** (`PlatformDbContext` in `Platform.Infrastructure/Persistence/PlatformDbContext.cs`). |
| **Migrations** | Live under `Platform.Infrastructure/Persistence/Migrations/` (e.g. `20260424063351_InitialCreate`). |
| **Startup** | `Program.cs` calls `Database.MigrateAsync()` in a scope **on every startup** (not limited to Development). Normative doc says **production** should prefer pipeline/job control — align operational runbooks when memory migrations become frequent. |
| **Seeding** | `OnModelCreating` uses `HasData` for several entities (dev/demo placeholders), including `MemoryInsight`. |
| **Extensions** | **pgvector is not** referenced in the codebase today; the master doc assumes it for embeddings — that will be a new migration/ops step when you adopt it. |

**Implication for memory:** New tables from the master doc (`memory_items`, `memory_events`, `semantic_memories`, etc.) ship as **EF migrations** against the same `PlatformDbContext` unless you later split databases (not assumed here).

---

## 3. Existing API conventions

- **Base path:** `/api/v1/...` (see `V1ApiRegistration.cs`).
- **JSON:** **camelCase** via `ConfigureHttpJsonOptions` in `Program.cs`.
- **Wire types:** Public request/response shapes live in **`Platform.Contracts`** (`*Request`, `*Dto` under `V1/`).
- **Routes:** One static class per area, `*V1Routes.Map(RouteGroupBuilder v1)`.
- **CORS / rate limiting / antiforgery:** Documented in `api-guidelines.md` (e.g. `.DisableAntiforgery()` for JSON posts where applicable); follow the same for new memory POSTs.
- **Errors:** Target **ProblemDetails** / structured validation (may be incremental) — new memory endpoints should not regress that direction.

---

## 4. Existing auth and user model

| Topic | Detail |
| --- | --- |
| **Gating** | **Platform access cookie** after `POST /api/admin/unlock` — this is **not** a user-identity / multi-user system (see `auth-and-security.md`). |
| **Middleware** | `RequirePlatformAccessMiddleware` — 401 without valid session; explicit bypasses for unlock/lock, optional public health, dev Swagger. |
| **“User” in data** | `PlatformProfile` and `PlatformUserSettings` are **singleton-keyed** demo-style rows (`Id = 1`), not a per-user `user_id` model. |

**vs master doc `user_id` columns:** For **single-tenant**, you can use a **single constant principal id** (e.g. `1` or a fixed UUID) in all `user_id` / `PrincipalId` columns, or drop `user_id` from the physical schema and scope by convention. The important part is **one clear row** in a `Principal` (or `MemorySubject`) table that every FK references so the model stays honest if you ever add multi-tenant later.

---

## 5. Background worker and Temporal patterns

| Layer | Pattern |
| --- | --- |
| **.NET** | **`IWorkflowStarter`** (`Platform.Application/Abstractions/Workflows/`) — implemented by `TemporalWorkflowStarter` when `Temporal:Address` is set, else **`StubWorkflowStarter`**. Workflows are **started** from `StartWorkflowRunCommandHandler` (e.g. product workflows keyed by `WorkflowRun`). |
| **Python** | **`workers-platform`**: Temporal **worker** process (`app/runtime/worker/main.py`), workflows under `app/workflows/`, with README stating workers **do not own** system-of-record state. |
| **Memory in Python** | `workers-platform/app/memory/` has a small **Protocol**-style `MemoryClient.fetch_context` — a placeholder for workflow-facing retrieval, not the full governed architecture yet. |
| **.NET hosted jobs** | No `BackgroundService` / scheduled **consolidation** job in the repo today. The master doc’s **nightly consolidation** naturally maps to a **Temporal schedule + Python worker** (or a new scheduled workflow), not necessarily to a long-running process inside `Platform.Api`. |

**Implication for memory:** Ingest/retrieval APIs and PostgreSQL truth stay in **`backend-platform`**. Heavy embedding/clustering/consolidation logic aligns with **Python + Temporal** as in the master doc, with clear **ports** and HTTP (or gRPC, if you add it later) boundaries — exact split is a decision (see questions).

---

## 6. Where the Memory bounded context should live

The repo **already** treats “Memory” as a vertical slice (placeholder):

| Layer | Path / note |
| --- | --- |
| **Domain** | `Platform.Domain/Features/Memory/` — *currently* a placeholder `MemoryInsight`; **scheduled for removal** in favor of master-doc entities (see [Locked product decisions](#locked-product-decisions)). |
| **Application** | `Platform.Application/…/Memory/` — replace list-insights with real memory use cases. |
| **Infrastructure** | `Platform.Infrastructure/Features/Memory/` — replace read repo with new repositories. |
| **Api** | `MemoryV1Routes.cs` — will map new routes as the feature grows. |
| **Contracts** | New `V1` DTOs for memory; remove `MemoryInsightDto` with the rest of the draft. |

**Recommendation (structural, not a rename mandate):** Keep the **Memory** feature as the home for the governed memory system. As complexity grows, **subfolders** are preferable to a new top-level `Features` name (e.g. `Memory/Insights`, `Memory/Context`, `Memory/Events`, `Memory/Profile` under Application/Abstractions/Features) **unless** you explicitly want a separate bounded context name (e.g. “Cognition”) — that would be a product/branding choice.

**Naming collision:** The master doc’s `memory_items` / “memory” terminology does not match the current **`MemoryInsight`** table name. Plan a deliberate **evolution** (new tables + deprecate, or migrate and rename) after you answer how `MemoryInsight` relates to `semantic_memories` / `memory_items` (see questions).

---

## Completed: Memory module structure (2026-04-26)

A **structure-only** pass landed (no new EF migrations, no vector search, no workers). It adds:

- **Domain:** `Entities/`, `Enums/`, `ValueObjects/`, and `Legacy/MemoryInsight.cs` (unchanged `Platform.Domain.Features.Memory` namespace for existing migrations).
- **Application:** abstractions per area; feature folders: `Events/IngestEvent/`, `Items/ListItems/`, `Context/GetMemoryContextShell/`, `ReviewQueue/ListPending/`, `Legacy/Insights/`; `AddMemoryApplication()` in `MemoryApplicationServiceCollectionExtensions`.
- **Infrastructure:** `Legacy/LegacyMemoryInsightsReadRepository`, `Stubs/` (no-op event writer, empty list repos, shell context assembler), `AddPlatformMemoryInfrastructure(IConfiguration)` (includes pgvector recall; see [12-vector-memory.md](12-vector-memory.md)).
- **Contracts:** `V1/Memory/*` for shell and summary DTOs; legacy `MemoryInsightDto` unchanged.
- **Api:** `Memory/Module/MemoryModuleV1Routes` (`GET memory/structure`), `Memory/Legacy/Insights/MemoryInsightsV1Routes` (`GET memory/insights`); `MemoryV1Routes` composes them.
- **Doc:** [MEMORY_MODULE_README.md](MEMORY_MODULE_README.md).

`GET /api/v1/memory/insights` remains for the current client; new use-case handlers are DI-ready without new public routes. **Legacy** (`MemoryInsight` table, insights route) is **kept** until a final migration pass removes it when governed memory is ready—do not delete it in intermediate steps unless the frontend and DB are updated together.

---

## 7. Naming and architecture standards (already in use)

- **Handlers / queries / commands:** `*QueryHandler`, `*CommandHandler`, `*Query`, `*Command` per `naming-conventions.md` and `backend-standards.md`.
- **Ports:** `I*ReadRepository` for table-shaped reads; `I*ReadModelSource` for composed reads; `I*Repository` for read/write aggregates — pick the **closest** existing feature as a template.
- **No EF or `HttpContext` in Application.**
- **No MediatR.**
- **Feature development checklist:** `feature-development-guide.md` (wire → domain + migration → port → infra + DI → handler → routes → security → tests).

---

## Phased task list (aligned to master roadmap + repo)

Phases follow the master doc **roadmap** but are **broken into implementable steps** in *this* repository. Durations are not estimated — you will drive cadence per phase in follow-up prompts.

### Phase 0 — Decisions and foundations (prerequisite)

- [ ] Resolve **identity / `user_id`** model vs singleton platform (see [Decisions and questions](#decisions-and-questions)).
- [ ] Confirm **persistence shape** for Phase 1: which master-doc tables ship first, and how they map to EF entity names.
- [ ] Agree **API contract** for `GetMemoryContext` (path, DTO field names, versioning).
- [ ] Agree **worker → API** authentication and deployment topology (if workers call the API to read/write system of record).

**Exit:** Written answers to the questions below; no guessing in implementation work.

### Phase 1 (master “Phase 1”) — PostgreSQL memory domain, profile, events, basic retrieval

**Backend (dotnet)**

- [ ] Add domain entities and enums for the **agreed** subset of tables (e.g. profile-related + `memory_events` + core `memory_items` or equivalent), following `Platform.Domain/Features/…` conventions.
- [ ] `PlatformDbContext`: `DbSet`s + `OnModelCreating` configuration; **EF migration** (and consider whether new seed data belongs in `HasData` or separate seed scripts).
- [ ] **Ports** in `Platform.Application/Abstractions/Memory/…` (read/write split as needed): e.g. append events, query items, composed read model for context.
- [ ] **Infrastructure** implementations in `Platform.Infrastructure/Features/Memory/…` + DI registration in `DependencyInjection.cs`.
- [ ] **Use cases:** `GetMemoryContext` (or agreed name) query + handler; optional command(s) for explicit profile updates if not merged into `PlatformProfile` flow.
- [ ] **API:** extend `MemoryV1Routes` (or add nested route maps) for new endpoints; register in `V1ApiRegistration` if you split route classes.
- [ ] **Tests:** unit tests for validators/handlers with mocked ports; integration tests for at least one happy path on `/api/v1/…`.

**Workers (python) — only if needed for Phase 1 scope**

- [ ] Either defer worker changes, or add **call paths** that emit events through the **API** (not direct DB) once event ingestion exists.

**Exit:** Persisted event ingestion + a working **curated** context response (per master “Retrieval Pipeline” subset you scope), behind the platform access model.

### Phase 2 (master “Phase 2”) — Semantic layer, consolidation, review queue

- [ ] Implement **`semantic_memories`**, `memory_evidence` (or agreed relational shape), linking to `memory_events` per master doc.
- [ ] **Review queue** entity + API (list/approve/reject) + Application rules for authority.
- [ ] **Nightly consolidation:** Temporal **scheduled** workflow in `workers-platform` (new workflow module) + activities in Python; **or** interim manual trigger for testing; define idempotency and how runs are observed (`WorkflowRun` pattern vs memory-specific run record — decision).
- [ ] **Operational story:** logging, failure handling, and how consolidation respects “Never Auto Save” rules from the master doc.

**Exit:** Proposals flow to review queue; safe auto-apply path implemented only for explicitly approved categories.

### Phase 3 (master “Phase 3”) — Procedural memory, ranking, context packets

- [ ] `procedural_rules` storage and versioning; API read path into `GetMemoryContext` pipeline.
- [ ] **Ranking** implementation (master `final_relevance` or simplified first pass) — likely **Python** for tunability, with scores/materialized results **stored or cached** in PostgreSQL as decided.
- [ ] Standardize **context packet** DTO for agents (Contracts + documentation).

**Exit:** Agents consume one stable **MemoryContext** shape; procedural rules included for scoped workflows.

### Phase 4 (master “Phase 4”) — Graph memory

- [ ] Introduce **graph store** (Neo4j or similar) **only after** the relational truth layer is stable; define sync or dual-write strategy.
- [ ] API: relationship exploration (as in master “Graph View”).

**Exit:** Graph is queryable; relational DB remains source of truth for auditable fields (per master “PostgreSQL, Source of Truth”).

### Phase 5 (master “Phase 5”) — Advanced adaptive memory

- [ ] Product metrics, decay tuning, self-optimizing workflow hooks — **depends on** production telemetry and user trust design.

**Exit:** TBD with product; not blocked on Phases 1–3.

---

## Cross-cutting work (any phase when needed)

- **Migrations in production:** align with `backend-standards.md` (pipeline/job vs instance startup) before high-risk schema changes.
- **Observability:** structured logs around memory writes and consolidation; request correlation if/when middleware is added.
- **Frontend / Memory Center UI:** the master doc describes a full UI; coordinate API contracts with whatever frontend package lives in the repo (out of scope for this plan unless you add a `frontend-*` workstream per phase).
- **Blob storage** for long-form “Document Memory” when you leave pure-Postgres text fields — not present as a first-class pattern in the snippets reviewed; introduce an abstraction in Application when you need it.

---

## Decisions and questions

**Resolved** — see [Locked product decisions](#locked-product-decisions) and [Recommendation: events ingest and workers](#recommendation-events-ingest-and-workers) (tenancy, normalized profile + memory, remove draft `MemoryInsight` surface, defer `GetMemoryContext` HTTP, API-owned writes + internal key for workers).

**Still open** (answer when a phase needs them):

### API and product surface

1. **Exact** route and DTO for **`GetMemoryContext`** (deferred, but pick before the agent-facing phase) — name, versioning, query params vs body, POST-only for rich task payloads or not.

### Infrastructure

2. **pgvector:** Target PostgreSQL version, extension enablement in **all** dev/prod environments, and whether embeddings are **always** written from Python or also from .NET.
3. **Blob storage** provider and when to move large artifacts out of PostgreSQL.
4. **Graph DB (Phase 4):** Any preference (Neo4j vs others) or hard constraint to stay on PostgreSQL-only until a scale threshold?

### Temporal and consolidation

5. Should the **nightly** job be a **dedicated** Temporal **scheduled workflow** in `workers-platform` (master doc’s direction), a **.NET**-hosted scheduler, or a **manual/cron** external process?
6. **Observability:** one `WorkflowRun` row per consolidation vs a dedicated `memory_job_runs` (or similar) table?

### Trust and policy

7. **Auto-apply vs require approval** — **data-driven** (config table) or **code-only** in v1?
8. **Compliance** (e.g. export/delete all memory) required in **Phase 1** schema or can wait?

---

## How to use this doc with follow-up phase prompts

For each phase, a practical prompt can include: **(a)** the phase number from this doc, **(b)** answers to any questions above that affect that phase, **(c)** acceptance tests / API examples you want, and **(d)** explicit **out of scope** items. That keeps implementation aligned with the [master architecture](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) without locking decisions prematurely.
