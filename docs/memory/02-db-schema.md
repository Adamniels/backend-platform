# Memory system — database schema v1

PostgreSQL. Migrations live in `backend-platform/src/Platform.Infrastructure/Persistence/Migrations/`. The v1 governed-memory migration is **`MemorySystemV1`** (`20260426115451_MemorySystemV1.cs`). The explicit user profile table is added in **`MemoryExplicitUserProfileV1`**.

**Conventions**

- **Table names** use `snake_case` via `ToTable("...")` on the governed memory tables (e.g. `memory_items`, `semantic_memories`).
- **Column names** follow EF / Npgsql defaults: **PascalCase** in the database (`"UserId"`, `"CreatedAt"`, …), matching the rest of this solution (e.g. `MemoryInsights`).
- **IDs:** `bigint` identity for all memory fact tables; `memory_users.id` is `integer` identity.
- **Enums** are stored as **`integer`**; see `Platform.Domain/Features/Memory/Enums/MemoryEnums.cs` and `SemanticMemoryStatus`.
- **JSON** payloads use **`jsonb`** (`StructuredJson`, `PayloadJson`, `ProposedChangeJson`, `EvidenceJson`).
- **Tenancy:** every governed row (except evidence join metadata) includes **`UserId` → `memory_users.Id`**. Single-tenant today: one seed row `Id = 1` (`MemoryUser.DefaultId` in code).
- **pgvector:** `memory_embeddings` + extension **`vector`** (migration **`MemoryEmbeddingsV1`**; see [12-vector-memory.md](12-vector-memory.md)). Neo4j remains out of scope.

---

## `memory_users`

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `integer` PK, identity | Seeded `1` for single-tenant. |
| `CreatedAt` | `timestamptz` | When the row was created. |

**Indexes:** PK only.

**Purpose:** Foreign-key anchor for per-user memory. When real auth exists, additional profile columns or a join to an `auth` user table can be added in a later migration.

---

## `memory_explicit_profile`

**User-entered** profile (highest authority; not written by inference). Migration **`MemoryExplicitUserProfileV1`**. See [05-profile-memory.md](05-profile-memory.md).

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | **Unique** (one row per user). |
| `CoreInterests` | `text[]` | |
| `SecondaryInterests` | `text[]` | |
| `Goals` | `text[]` | |
| `Preferences` | `jsonb` | Array of `{ "key", "value" }`. |
| `ActiveProjects` | `jsonb` | Array of `{ "name", "externalId"? }`. |
| `SkillLevels` | `jsonb` | Array of `{ "name", "level" }` (`level` 0.0–1.0 in domain validation). |
| `AuthorityWeight` | `double precision` | 1.0 for user saves. |
| `CreatedAt` | `timestamptz` | |
| `UpdatedAt` | `timestamptz` | |

**Indexes:** unique on `UserId` (`IX_memory_explicit_profile_UserId`).

---

## `memory_items`

Maps to master **memory_items** (canonical items: profile facts, notes, inferred items, etc.).

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | |
| `MemoryType` | `integer` | `MemoryItemType` enum. |
| `Title` | `varchar(512)` | |
| `Content` | `text` | |
| `StructuredJson` | `jsonb` | Nullable. |
| `SourceType` | `varchar(64)` | Origin classification (string, not enum, for flexibility). |
| `ProjectId` | `varchar(256)` | Nullable; document scoping and vector recall filter (migration **`DocumentMemoryV1`**). |
| `Domain` | `varchar(256)` | Nullable; topical scope (migration **`DocumentMemoryV1`**). |
| `AuthorityWeight` | `double precision` | 0.0–1.0 scale (see master authority model). |
| `Confidence` | `double precision` | |
| `Importance` | `double precision` | |
| `FreshnessScore` | `double precision` | |
| `Status` | `integer` | `MemoryItemStatus` enum. |
| `CreatedAt` | `timestamptz` | |
| `UpdatedAt` | `timestamptz` | |
| `LastAccessedAt` | `timestamptz` | Nullable. |

**Indexes**

- `ix_memory_items_user_id` — `UserId`
- `ix_memory_items_user_id_status` — `(UserId, Status)` — filter by lifecycle per user
- `ix_memory_items_user_id_memory_type` — `(UserId, MemoryType)` — typed slices
- `ix_memory_items_user_id_project_id` — `(UserId, ProjectId)` — document / scoped listings
- `ix_memory_items_user_id_domain` — `(UserId, Domain)` — domain slices
- `ix_memory_items_created_at` — `CreatedAt` — time-ordered listings

---

## `memory_embeddings`

**pgvector** support for governed **`memory_items`** recall. Migrations **`MemoryEmbeddingsV1`** (table + HNSW) and **`DocumentMemoryV1`** (chunking columns + unique index). See [12-vector-memory.md](12-vector-memory.md) and [13-document-memory.md](13-document-memory.md).

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | |
| `MemoryItemId` | `bigint` FK → `memory_items` | **Cascade** delete when the item is removed. |
| `EmbeddingModelKey` | `varchar(256)` | Logical model identifier. |
| `EmbeddingModelVersion` | `varchar(64)` | Nullable provider version. |
| `Dimensions` | `integer` | Must match `vector(N)` width (v1: **1536**). |
| `ContentSha256` | `varchar(64)` | Hex digest of canonical embedded text. |
| `ChunkIndex` | `integer` | 0-based chunk index (`DocumentMemoryV1`); non-document items use **0**. |
| `EmbeddedText` | `text` | Nullable; chunk or whole-item text used for recall previews. |
| `Embedding` | `vector(1536)` | pgvector column. |
| `CreatedAt` | `timestamptz` | |
| `UpdatedAt` | `timestamptz` | |

**Indexes**

- **`ix_memory_embeddings_user_item_model_chunk`** — **UNIQUE** `(UserId, MemoryItemId, EmbeddingModelKey, ChunkIndex)`
- **`ix_memory_embeddings_embedding_hnsw`** — **HNSW** on `Embedding` with `vector_cosine_ops`
- `IX_memory_embeddings_MemoryItemId` — FK helper

---

## `memory_events`

Append-only **episodic** log (master **memory_events**).

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | |
| `EventType` | `varchar(256)` | |
| `Domain` | `varchar(256)` | Nullable. |
| `WorkflowId` | `varchar(256)` | Nullable. |
| `ProjectId` | `varchar(256)` | Nullable. |
| `PayloadJson` | `jsonb` | Nullable. |
| `OccurredAt` | `timestamptz` | When the event happened in the world. |
| `CreatedAt` | `timestamptz` | When the row was written (ingestion / server time). |

**Indexes**

- `ix_memory_events_user_id` — `UserId`
- `ix_memory_events_user_id_occurred_at` — `(UserId, OccurredAt)` — timeline per user
- `ix_memory_events_user_id_event_type` — `(UserId, EventType)` — filter by type

**Note:** `Status` is intentionally omitted here; if you need processing state (dead-letter, retry), add a nullable status or a separate outbox in a later migration.

---

## `semantic_memories`

Learned **claims** (master **semantic_memories**).

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | |
| `Key` | `varchar(256)` | Stable key; dedupe for **active + pending** enforced in app + unique partial index (see [08-semantic-memory.md](08-semantic-memory.md)). |
| `Claim` | `text` | |
| `Domain` | `varchar(256)` | Nullable. |
| `Confidence` | `double precision` | |
| `AuthorityWeight` | `double precision` | |
| `Status` | `integer` | `SemanticMemoryStatus` enum. |
| `CreatedAt` | `timestamptz` | |
| `UpdatedAt` | `timestamptz` | |
| `LastSupportedAt` | `timestamptz` | Nullable. |

**Indexes**

- `ix_semantic_memories_user_id` — `UserId`
- `ix_semantic_memories_user_id_status` — `(UserId, Status)`
- `ix_semantic_memories_user_id_key` — `(UserId, Key)` — lookup by key
- `ix_semantic_memories_user_key_domain_active_pending` — **unique partial** on `(UserId, lower("Key"), coalesce("Domain", ''))` where `Status IN (1, 4)` (migration `SemanticMemoryDedupIndexV1`)

---

## `memory_consolidation_runs`

Append-only style **run log** for the nightly (or ad-hoc) consolidation job. Migration **`MemoryConsolidationRunsV1`**. See [10-nightly-worker.md](10-nightly-worker.md).

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | Multi-user ready; v1 worker defaults to configured primary user. |
| `WindowStart` | `timestamptz` | Inclusive lower bound for `memory_events.OccurredAt`. |
| `WindowEnd` | `timestamptz` | **Exclusive** upper bound (same convention as code queries). |
| `IdempotencyKey` | `varchar(256)` | **Unique** (e.g. `nightly-{userId}-{yyyy-MM-dd}`). |
| `ProcessedEventsCount` | `integer` | Events scanned in window. |
| `ProposalsCreatedCount` | `integer` | New review-queue rows created this run. |
| `AutoUpdatesCount` | `integer` | Inferred-safe semantic updates (e.g. reinforce) this run. |
| `Status` | `integer` | `MemoryConsolidationRunStatus`: Running, Completed, Failed. |
| `Error` | `varchar(8000)` | Nullable; set when `Failed`. |
| `StartedAt` | `timestamptz` | |
| `CompletedAt` | `timestamptz` | Nullable until terminal state. |

**Indexes**

- `ix_memory_consolidation_runs_idempotency_key` — **unique** on `IdempotencyKey`
- `ix_memory_consolidation_runs_user_started` — `(UserId, StartedAt)`

---

## `memory_evidence`

Links **semantic** claims to **events** (master **memory_evidence**), with optional per-user scoping for efficient filtering.

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | Denormalized for index-friendly tenant filters. |
| `SemanticMemoryId` | `bigint` FK → `semantic_memories` | **ON DELETE CASCADE** from semantic. |
| `EventId` | `bigint` FK → `memory_events` | **ON DELETE RESTRICT** (keep history unless you delete the event explicitly). |
| `Strength` | `double precision` | |
| `Reason` | `varchar(2048)` | Nullable. |
| `CreatedAt` | `timestamptz` | When the link was recorded. |

**Indexes**

- `ix_memory_evidence_user_id` — `UserId`
- `ix_memory_evidence_semantic_memory_id` — `SemanticMemoryId`
- `ix_memory_evidence_event_id` — `EventId`
- `ix_memory_evidence_user_semantic` — `(UserId, SemanticMemoryId)`

---

## `procedural_rules`

Versioned **how to behave** rules (master **procedural_rules**).

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | |
| `WorkflowType` | `varchar(128)` | e.g. learning, digest generation. |
| `RuleName` | `varchar(256)` | |
| `RuleContent` | `text` | e.g. markdown / structured text. |
| `Priority` | `integer` | |
| `Source` | `varchar(512)` | Provenance. |
| `AuthorityWeight` | `double precision` | 0.0–1.0; ranking in `GetMemoryContext` and review gate (see [11-procedural-memory.md](11-procedural-memory.md)). |
| `Version` | `integer` | Monotonic per `(UserId, WorkflowType, RuleName)`. |
| `Status` | `integer` | `ProceduralRuleStatus` enum. |
| `CreatedAt` | `timestamptz` | |
| `UpdatedAt` | `timestamptz` | |

**Indexes**

- `ix_procedural_rules_user_workflow_status` — `(UserId, WorkflowType, Status)` — load active rules for a workflow
- **`ix_procedural_rules_user_rule_name_version`** — **UNIQUE** `(UserId, WorkflowType, RuleName, Version)` — one row per version

---

## `memory_review_queue`

Items awaiting user approval (master **memory_review_queue**).  
**Status** + **UpdatedAt** support workflow (pending → approved / rejected) without a separate state machine table in v1.

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | |
| `ProposalType` | `integer` | `MemoryReviewProposalType` enum. |
| `Title` | `varchar(512)` | |
| `Summary` | `varchar(4000)` | |
| `ProposedChangeJson` | `jsonb` | Nullable. |
| `EvidenceJson` | `jsonb` | Nullable. |
| `Priority` | `integer` | Higher = more urgent. |
| `Status` | `integer` | `MemoryReviewStatus` enum. |
| `CreatedAt` | `timestamptz` | |
| `UpdatedAt` | `timestamptz` | Set when status changes. |
| `ApprovedSemanticMemoryId` | `bigint` nullable FK → `semantic_memories` | Set on **approve** for `NewSemantic` (ON DELETE SET NULL). Migration **`MemoryReviewQueueAuditV1`**. |
| `ApprovedProceduralRuleId` | `bigint` nullable FK → `procedural_rules` | Set on **approve** for `NewProceduralRule` (ON DELETE SET NULL). Migration **`ProceduralMemoryAuthorityV1`**. See [11-procedural-memory.md](11-procedural-memory.md). |
| `RejectedReason` | `varchar(2000)` | Optional on **reject**. |
| `ResolvedAt` | `timestamptz` | When approved or rejected. |
| `ReviewNotes` | `varchar(4000)` | Optional audit notes on approve. |

**Indexes**

- `ix_memory_review_queue_user_id_status_priority` — `(UserId, Status, Priority)` — work queue per user
- `IX_memory_review_queue_ApprovedSemanticMemoryId` — FK index on `ApprovedSemanticMemoryId`
- `IX_memory_review_queue_ApprovedProceduralRuleId` — FK index on `ApprovedProceduralRuleId` (**ProceduralMemoryAuthorityV1**)
- `ix_memory_review_queue_created_at` — `CreatedAt` — triage by age

---

## `memory_relationships`

**Graph-lite** entity edges (master **memory_relationships**). Stored as strings + enum for v1; **not** a graph database.

| Column | Type | Notes |
| --- | --- | --- |
| `Id` | `bigint` PK | |
| `UserId` | `integer` FK → `memory_users` | |
| `FromEntity` | `varchar(512)` | Identifier or display key (v1; future stable IDs in another migration). |
| `RelationType` | `integer` | `MemoryRelationshipType` enum. |
| `ToEntity` | `varchar(512)` | |
| `Strength` | `double precision` | |
| `Source` | `varchar(512)` | Nullable provenance. |
| `CreatedAt` | `timestamptz` | |

**Indexes**

- `ix_memory_relationships_user_id_from` — `(UserId, FromEntity)` — “what points out of this node?”
- `ix_memory_relationships_user_id_to` — `(UserId, ToEntity)` — “what points in?”
- `ix_memory_relationships_user_id_relation_type` — `(UserId, RelationType)` — filter by edge type

---

## Legacy (unchanged in this v1)

The first-draft **`MemoryInsights`** table, **`GET /api/v1/memory/insights`**, and related code remain until a dedicated removal (see [LEGACY_MEMORY_REMOVAL.md](LEGACY_MEMORY_REMOVAL.md)). It is **not** part of the governed `memory_*` set above.

---

## C# / EF entry points

- **Entities:** `Platform.Domain/Features/Memory/Entities/`
- **EF configuration:** `Platform.Infrastructure/Persistence/MemoryV1EfConfiguration.cs` (`ConfigureMemoryV1`)
- **DbContext:** `PlatformDbContext` registers `DbSet<>`s and calls `modelBuilder.ConfigureMemoryV1()` at the end of `OnModelCreating`.

For higher-level product behavior, see [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) and [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md).
