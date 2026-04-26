# Retrieval engine — GetMemoryContext v1

## Purpose

Agents and workflows should call **`GetMemoryContext`** to receive a **curated memory packet** (ranked, bounded lists), not raw table dumps. v1 uses **SQL filtering** (EF Core `Where` + limits), **deterministic in-process scoring**, and **optional pgvector recall** over governed `memory_embeddings` (see [12-vector-memory.md](12-vector-memory.md)). **No graph traversal** over `memory_relationships` in this path.

## API

- **POST** `POST /api/v1/memory/context` (unlocked platform session, JSON; `.DisableAntiforgery()` like other memory writes).
- Body: `GetMemoryContextV1Request` (camelCase JSON):
  - `userId` — optional; `0` / omitted → default `memory_users` id `1`.
  - `taskDescription` — optional; drives token overlap for relevance (max length validated in `GetMemoryContextV1RequestValidator`).
  - `workflowType` — optional; boosts procedural rules and episodic rows that match.
  - `projectId` — optional; boosts explicit **active projects** and **memory_events** with the same `ProjectId`.
  - `domain` — optional; filters/boosts `semantic_memories.Domain` and `memory_events.Domain` when set.

Response: `MemoryContextV1Dto` with:

| Field | Source (high level) |
| --- | --- |
| `profileFacts` | `memory_explicit_profile` (core/secondary interests, preferences) — **highest authority**; rank floor in code. |
| `activeGoals` | Explicit profile `Goals` (`text[]`) |
| `relevantProjects` | Explicit profile `ActiveProjects` jsonb; project id match adds weight |
| `semanticMemories` | `semantic_memories` — `Active` or `PendingReview` only |
| `episodicExamples` | Recent `memory_events` (capped, then re-ranked) |
| `proceduralRules` | `procedural_rules` — `Active` only; **latest version** per `(WorkflowType, RuleName)`; each DTO includes `Source` and `AuthorityWeight` (see [11-procedural-memory.md](11-procedural-memory.md)) |
| `memoryItemVectorRecalls` | `memory_embeddings` → `memory_items` (Active); optional pgvector cosine when a query embedding is available; document hits include chunk index and scope metadata (see [12-vector-memory.md](12-vector-memory.md), [13-document-memory.md](13-document-memory.md)) |
| `conflicts` | v1: duplicate `semantic_memories` key with **distinct** claims (optional data-dependent) |
| `warnings` | e.g. missing explicit profile row, empty task description |

`assemblyStage` is **`v1-sql`** when vector recall is off or unavailable, or **`v1-sql+vector`** when recall ran with hits (see [12-vector-memory.md](12-vector-memory.md)).

## Scoring (v1)

Implementation: `MemoryContextV1Scoring` in `Platform.Application/Features/Memory/Context/MemoryContextV1Scoring.cs` (unit-tested). Factors combined per slice:

- **Authority** — explicit profile uses `1.0`; semantic uses row `AuthorityWeight`; events use a neutral default; procedural rules use the row’s **`AuthorityWeight`** (persisted; previously a fixed constant in v1 SQL).
- **Confidence** — semantic `Confidence`; other slices use fixed neutral values where the column does not exist.
- **Recency** — exponential half-life (semantic ~45d updated-at, events ~20d occurred-at, rules ~120d).
- **Workflow relevance** — string match / substring between request `workflowType` and row (`memory_events.WorkflowId`, `procedural_rules.WorkflowType`, etc.).
- **Project relevance** — equality on `ProjectId` where present.
- **Text relevance** — tokenized `taskDescription` vs key fields (simple substring hit ratio over tokens).
- **Status** — `SemanticMemoryStatus` maps to a multiplicative factor; archived/superseded semantics are excluded at SQL level.
- **Domain** — when `domain` is provided, rows with a matching `Domain` get a higher `domainMatch` term.

**Explicit profile priority:** profile-derived items use `ExplicitProfileItemRank` so that even weak text matches stay **above** typical **inferred** semantic rows in comparable conditions (see unit test `Explicit_profile_rank_floor_...`).

## Implementation map

- **Provider:** `EfMemoryContextProvider` (`IMemoryContextProvider`) — `Platform.Infrastructure/Features/Memory/Context/`.
- **HTTP entry:** `PostMemoryContextRequestHandler` → `GetMemoryContextQueryHandler` (validates request, then builds `MemoryContextRequest`).

## Not in v1

- Neo4j / `memory_relationships` graph walks in `GetMemoryContext`
- Learned re-ranking or LLM-based fusion

## See also

- [02-db-schema.md](02-db-schema.md) — tables
- [05-profile-memory.md](05-profile-memory.md) — explicit profile source
- [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) — product authority principles
