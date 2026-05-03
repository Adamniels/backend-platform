# Nightly memory consolidation — worker v1 (implemented)

This document describes the **first shipped** nightly consolidation slice: deterministic rules over **yesterday’s** `memory_events`, **internal service auth**, **run logging**, and a **Python Temporal worker** on a **dedicated task queue**. It implements the decisions recorded in [09-nightly-consolidation-design.md](09-nightly-consolidation-design.md).

**Out of scope v1:** LLM extraction, multi-day windows (7/30/90), vector clustering, procedural-rule auto writers.

---

## Architecture

| Component | Role |
| --- | --- |
| **`backend-platform`** | System of record: reads `memory_events`, updates `semantic_memories` / `memory_evidence`, creates `memory_review_queue` rows, persists `memory_consolidation_runs`. |
| **`workers-platform`** | Temporal **worker** hosts `MemoryConsolidationWorkflow` on queue **`memory-consolidation`** (default); activity calls the internal HTTP API. |
| **Temporal** | **Schedule** (or manual `workflow start`) targets the **memory-consolidation** queue so consolidation never shares the default `platform` queue with interactive workflows. |

---

## Internal HTTP API (.NET)

**Path:** `POST /api/internal/v1/memory/consolidation/nightly`

**Authentication:** `Authorization: Bearer <MemoryWorker:ServiceToken>` (see `appsettings.json` / secrets). **Not** the browser unlock cookie. Middleware `InternalMemoryWorkerAuthenticationMiddleware` validates the token **only** for `/api/internal/v1/memory/*`. If `ServiceToken` is empty, internal routes return **503**.

**Platform session:** After successful bearer validation, `RequirePlatformAccessMiddleware` is bypassed via `HttpContext.Items` so workers do not need `POST /api/admin/unlock`.

**Body (`ExecuteNightlyMemoryConsolidationV1Request`):**

| Field | Default | Notes |
| --- | --- | --- |
| `userId` | `MemoryWorker:PrimaryUserId` (default `1`) | Multi-user ready; v1 processes one configured user. |
| `windowEndExclusiveUtc` | Today’s UTC calendar date | Window is `[WindowEnd - 1d, WindowEnd)` in UTC. |
| `idempotencyKey` | `nightly-{userId}-{windowEnd:yyyy-MM-dd}` | **Unique** in `memory_consolidation_runs`; completed runs short-circuit (`fromCache: true`). |

**Response (`NightlyMemoryConsolidationV1Response`):** run id, counts, `fromCache`, `status`, optional `error`.

**Testing:** `ASPNETCORE_ENVIRONMENT=Testing` returns richer `ProblemDetails` for unhandled exceptions (developer aid only).

---

## Deterministic rules (v1)

1. Load up to **N** events in the window (`IMemoryConsolidationPolicyProvider.MaxEventsPerWindow`).
2. Group by **`EventType`** (case-insensitive). If count ≥ **`MinOccurrencesForPattern`** (default **3**):
   - Derive semantic **key** `consolidation.event.{sanitizedType}` (`MemoryConsolidationKeys`).
   - **If** an **Active** or **PendingReview** semantic exists for that key + modal domain: **attach** one representative event as `memory_evidence`, **reinforce** confidence by **`ReinforceConfidenceDelta`**, `fromInferredSource: true`, **unless** `AuthorityWeight` is at/above the inferred override ceiling (see [08-semantic-memory.md](08-semantic-memory.md)) or the evidence link already exists.
   - **Else:** enqueue **`NewSemantic`** review item with a neutral claim (no direct long-term semantic row). **Idempotency:** skip if a **pending** item already has the same **`consolidationFingerprint`** substring in `EvidenceJson` (in-memory scan of pending evidence strings for v1 to avoid `jsonb ~~ jsonb` SQL issues).

**Never:** writes to **`memory_explicit_profile`** or silent creation of high-stakes semantics without review.

---

## Policy interface

`IMemoryConsolidationPolicyProvider` + `DefaultMemoryConsolidationPolicyProvider` hold **code constants** today; swap for DB-backed policy later without scattering literals in handlers.

---

## Run record

Table **`memory_consolidation_runs`** ([02-db-schema.md](02-db-schema.md)): `window_start`, `window_end` (exclusive end), counts, `status`, `error`, timestamps, **`idempotency_key`** (unique). **Failed** runs may be retried with the same key (row reset to `Running`); **Completed** runs are idempotent for that key.

---

## Python worker

**Process:** `uv run python -m app.runtime.worker.main` registers **two** `temporalio.worker.Worker` instances:

1. **`TEMPORAL_TASK_QUEUE`** (default `platform`) — existing product workflows.
2. **`TEMPORAL_TASK_QUEUE_MEMORY`** (default `memory-consolidation`) — `MemoryConsolidationWorkflow` + `run_nightly_memory_consolidation_activity`.

**Environment** (see `workers-platform/.env.example`):

- `PLATFORM_API_BASE_URL` — e.g. `http://localhost:5120` (matches `Platform.Api` http profile).
- `PLATFORM_INTERNAL_SERVICE_TOKEN` (legacy alias `MEMORY_WORKER_SERVICE_TOKEN`) — shared Bearer for all internal worker HTTP calls; must match `PlatformWorkers:ServiceToken` on the API host.
- `CONSOLIDATION_PRIMARY_USER_ID` — default `1`.

**Activity** uses **httpx** `POST` with JSON body `{ "userId": … }` and logs status / counts.

---

## Temporal scheduling

Create a **Temporal Schedule** targeting workflow type **`MemoryConsolidationWorkflow`** on task queue **`memory-consolidation`** (CLI/UI depends on your Temporal deployment). Example sketch:

```bash
temporal schedule create \
  --schedule-id memory-nightly-utc \
  --interval 24h \
  --task-queue memory-consolidation \
  --type MemoryConsolidationWorkflow
```

Adjust timezone/calendar policy to match product (UTC midnight is a common default).

---

## Operations checklist

1. Set **`PlatformWorkers:ServiceToken`** on the API and the same secret as **`PLATFORM_INTERNAL_SERVICE_TOKEN`** (or legacy **`MEMORY_WORKER_SERVICE_TOKEN`**) on workers (secret manager in prod).
2. Run DB migrations (`memory_consolidation_runs`).
3. Deploy worker with both queues; verify Temporal schedule points at **`memory-consolidation`**.
4. Watch API logs (`ExecuteNightlyMemoryConsolidationCommandHandler`) and Temporal run history.

---

## See also

- [08-semantic-memory.md](08-semantic-memory.md) — inferred override rules used by consolidation.
- [07-review-queue.md](07-review-queue.md) — `NewSemantic` approval path.
- [09-nightly-consolidation-design.md](09-nightly-consolidation-design.md) — broader multi-window design for later phases.
