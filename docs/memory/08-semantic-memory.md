# Semantic memory (v1)

## Purpose

**Semantic memories** are **learned claims** (key, natural-language claim, optional domain) with **confidence** and **authority weight**, stored in `semantic_memories` and **always linked to episodic evidence** in `memory_evidence` (event id + strength + optional reason) when using the **management API** in this version.

v1 does **not** include an LLM extraction worker, **pgvector**, or hybrid retrieval; see [06-retrieval-engine.md](06-retrieval-engine.md) for how semantics participate in the SQL-backed **MemoryContext** ranker.

## Status model

`SemanticMemoryStatus` (integer in PostgreSQL) includes at least:

| Value | Name | Role |
| --- | --- | --- |
| `1` | `Active` | Participates in context and listings (subject to other filters). |
| `4` | `PendingReview` | Lower rank in context; can be approved into higher authority or rejected. |
| `3` | `Archived` | Stale / user-archived; excluded from “active” lists and context. |
| `5` | `Rejected` | User rejected a pending item; excluded from context. |
| `2` | `Superseded` | Replaced by a newer claim (other flows). |

## Evidence rule

- **API-managed creates** (`POST /api/v1/memory/semantics`) **require** a `eventId` that exists for the same `userId`, and create a **row in `memory_evidence`** for that semantic.
- **Review-queue approval** may create a semantic from a proposal without going through the same “attach evidence” path; that is a **documented exception** until a follow-up links review evidence to `memory_evidence` consistently.

## Authority and “inferred must not override”

- **Explicit user profile** (see [05-profile-memory.md](05-profile-memory.md)) remains the **highest-authority** slice for user-entered goals and preferences; **inference must not write** to that table in v1.
- For **semantic rows**, any operation marked as **inferred** (`fromInferredSource: true` on confidence updates or evidence attach) **must not** mutate rows whose `AuthorityWeight` is at or above `AuthorityWeight.InferredOverrideCeiling` (same value as `UserApprovedSemantic`, 0.92). That keeps user-approved or near-explicit semantics from being overwritten by low-trust automation. Direct user calls with `fromInferredSource: false` are not treated as inferred.

## Duplicate prevention

- **Application** checks for an existing **Active** or **PendingReview** row with the same **user**, **case-insensitive key**, and **normalized domain** (empty/null domains match each other) before insert.
- **Database:** partial **unique** index on `(UserId, lower("Key"), coalesce("Domain", ''))` where `Status IN (1, 4)` (migration `SemanticMemoryDedupIndexV1`) catches races.

## “Similar” lookup (no vectors)

`GET /api/v1/memory/semantics/find` filters **Active** + **PendingReview** with optional **domain equality** and optional **key substring** (`Contains`, case-insensitive). This is a **pragmatic** pre-check for workers that will use embeddings later—not semantic similarity.

## HTTP (v1)

Session must be **unlocked** (same as other `/api/v1/memory/...` JSON routes). Optional `?userId=` (omit or `0` → default `memory_users` id `1`).

| Method | Path | Summary |
| --- | --- | --- |
| `GET` | `/api/v1/memory/semantics` | List actives (optional `includePending`, default `true`). |
| `GET` | `/api/v1/memory/semantics/{id}` | Get one row for the user. |
| `GET` | `/api/v1/memory/semantics/find` | Similarity helper: `key`, `domain`, `take`. |
| `POST` | `/api/v1/memory/semantics` | Create with **required** `eventId` + evidence fields; body `CreateSemanticMemoryV1Request`. **409** if duplicate. |
| `PUT` | `/api/v1/memory/semantics/{id}/confidence` | Set confidence; `fromInferredSource` obeys floor. |
| `POST` | `/api/v1/memory/semantics/{id}/evidence` | Attach `memory_evidence`; optional reinforce delta. |
| `POST` | `/api/v1/memory/semantics/{id}/archive` | Archive (stale). |
| `POST` | `/api/v1/memory/semantics/{id}/reject` | `PendingReview` → `Rejected`. |

**409 Conflict** is returned for duplicate key/domain (see `MemoryConflictException` in the API host).

## Code map

- **Port:** `ISemanticMemoryService` — `Platform.Application/Abstractions/Memory/Semantic/ISemanticMemoryService.cs`
- **Implementation:** `EfSemanticMemoryService` — `Platform.Infrastructure/Features/Memory/Semantic/`
- **Read summaries:** `ISemanticMemoryReadRepository` / `EfSemanticMemoryReadRepository` (list DTOs for UIs)
- **Handlers:** `Platform.Application/Features/Memory/Semantic/**`
- **Routes:** `Platform.Api/Features/Memory/Semantic/SemanticMemoryV1Routes.cs`

## See also

- [02-db-schema.md](02-db-schema.md) — `semantic_memories`, `memory_evidence`, indexes
- [03-domain-model.md](03-domain-model.md) — entities and ports
- [05-profile-memory.md](05-profile-memory.md) — explicit profile vs learned semantics
- [07-review-queue.md](07-review-queue.md) — approval path for proposed semantics
