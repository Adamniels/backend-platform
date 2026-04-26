# Nightly memory consolidation — design (pre-implementation)

This document **designs** the **nightly consolidation** flow described in [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) (“Nightly Consolidation Engine”, “Auto Apply vs Approval Rules”, “Memory Decay and Evolution”) and aligns it with **existing repo infrastructure** ([IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) §5, `workers-platform`, `backend-platform` Temporal starter). **No implementation** is specified here beyond naming boundaries and decisions to make later.

**Related governed memory docs:** [02-db-schema.md](02-db-schema.md), [03-domain-model.md](03-domain-model.md), [06-retrieval-engine.md](06-retrieval-engine.md), [07-review-queue.md](07-review-queue.md), [08-semantic-memory.md](08-semantic-memory.md). **Implemented worker v1:** [10-nightly-worker.md](10-nightly-worker.md).

---

## 1. Goals and non-goals

**Goals**

- Periodically **read episodic evidence**, compare it to **existing semantics / profile / procedural hints**, and produce **bounded updates**: safe auto-applies, review-queue proposals, merges, and decay.
- Respect **explicit profile** as highest authority and **never** overwrite it from consolidation ([master “Conflict Handling”](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md)).
- Run on a **predictable schedule** with **durable execution**, retries, and visibility suitable for operations.

**Non-goals (this design pass)**

- Concrete embedding models, cluster algorithms, or SQL.
- Final choice of **observability** row shape (`WorkflowRun` vs `memory_consolidation_runs`) — options below.
- Implementing new HTTP routes or workers (tracked in implementation plan Phase 2).

---

## 2. Scheduling topology (recommended)

**Master direction:** “Run nightly via **Temporal scheduled workflow**. **Python workers** recommended.”

**Repo today**

| Piece | Location / behavior |
| --- | --- |
| **.NET → Temporal** | `IWorkflowStarter` + `TemporalWorkflowStarter` when `Temporal:Address` is set; used from product flows such as `StartWorkflowRunCommandHandler` (persists `WorkflowRun.TemporalWorkflowId`). |
| **Python worker** | `workers-platform`: `app/runtime/worker/main.py` registers workflows/activities from `app/runtime/registry/definitions.py` (today: `news_intelligence`, `side_learning`). |
| **.NET consolidation host** | **No** `BackgroundService` / in-process nightly job today ([IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) §5). |

**Recommendation**

1. **Temporal Schedule** (or namespace-level schedule) fires once per calendar **night** (timezone policy: **UTC** unless product specifies user-local).
2. A **new** Python workflow, e.g. `MemoryConsolidationWorkflow`, runs on the existing **`platform`** task queue (same as `TEMPORAL_TASK_QUEUE` in workers-platform README) **or** a dedicated `memory-consolidation` queue if isolation is needed later.
3. **Heavy work** (embeddings, clustering, summarization, proposal drafting) stays in **Python activities**; **all writes to PostgreSQL** go through **`backend-platform` HTTP APIs** (or a future internal gRPC port) so the .NET app remains **system of record** and policy enforcement stays centralized.

**Alternatives** (documented for decision [IMPLEMENTATION_PLAN Q5](IMPLEMENTATION_PLAN.md#temporal-and-consolidation))

- **.NET-hosted** `IHostedService` + cron: simpler if Temporal is absent in an environment, but duplicates scheduling/retry semantics and splits “nightly” logic away from existing Python worker investment.
- **External cron** invoking a one-shot CLI: acceptable for dev/staging smoke tests; weaker story for retries and run history unless wrapped by Temporal anyway.

---

## 3. Temporal workflow shape (if Temporal is used)

**Proposed workflow:** `MemoryConsolidationWorkflow` (Python, `workers-platform/app/workflows/memory_consolidation/` — *new module when implemented*).

**High-level structure**

```text
Schedule → MemoryConsolidationWorkflow.run(ConsolidationRequest)
  ├─ activity: load_consolidation_config (optional; versioning / feature flags)
  ├─ activity: resolve_targets (user ids / tenants to process this run)
  └─ foreach target (sequential or limited parallelism per DB safety):
        ├─ activity: fetch_event_windows (.NET-backed read)
        ├─ activity: build_feature_bundle (Python: token stats, optional embeddings)
        ├─ activity: detect_patterns (Python: clustering / heuristics)
        ├─ activity: diff_against_memory_state (.NET-backed read: semantics, profile summaries, rules)
        ├─ activity: plan_actions (pure: partition auto vs review vs skip)
        ├─ activity: apply_auto_actions (.NET-backed writes, idempotent keys)
        └─ activity: enqueue_review_items (.NET-backed writes)
```

**Temporal concerns**

- **Child workflows** per user are optional; start with **one workflow per schedule** and **activities** that batch users to avoid workflow history explosion.
- **Timeouts:** long activities for “read 90d window” need generous `start_to_close`; consider **pagination** inside activities (cursor by `OccurredAt` / `Id`).
- **Idempotency:** each planned mutation carries a deterministic **`ConsolidationRunId`** + **`MutationId`** (hash of user + window + proposal kind + normalized key) so retries do not double-apply.
- **Signals / queries:** optional later (`query` for “last progress”) if ops needs live status; not required for v1 design.

**.NET side:** today consolidation is **not** started from `IWorkflowStarter`; the schedule would target the **Python worker** directly. Optionally, a future **admin API** could `StartAsync` a one-off consolidation workflow for debugging—same workflow type, different trigger.

---

## 4. Time windows (daily, 7d, 30d, 90d)

Per master spec, consolidation consumes **multiple rolling windows** over **`memory_events`** (and optionally other append-only sources later).

| Window | Inclusive range (conceptual) | Primary use |
| --- | --- | --- |
| **Previous day** | `[startOfUtcDay-1d, startOfUtcDay)` | Fresh “what changed yesterday” signals; spike detection; immediate follow-ups. |
| **7 day** | `[now-7d, now)` | Short trends; weekly rhythm; low-latency preference shifts. |
| **30 day** | `[now-30d, now)` | Medium habits; project-scoped bursts vs sustained interest. |
| **90 day** | `[now-90d, now)` | Longer arcs; “persistent topic trend over weeks” class signals (master: approval-heavy). |

**Overlap:** windows are **nested**, not mutually exclusive. The planner should **tag** each derived signal with **which window(s)** supported it so downstream rules can require “seen in 7d **and** 30d” before escalating to approval.

**Clock:** use a single **`asOf`** timestamp captured at workflow start for all window boundaries so one run is internally consistent.

---

## 5. Which events are read

**Primary source (implemented today)**

- Table **`memory_events`** ([02-db-schema.md](02-db-schema.md)): `UserId`, `EventType`, `Domain`, `WorkflowId`, `ProjectId`, `PayloadJson`, `OccurredAt`, `CreatedAt`.

**Read model**

- For each `(UserId, window)`, load events ordered by **`OccurredAt`** (secondary **`Id`**), with **optional caps** per window (e.g. max N rows or byte budget) to protect workers.
- **Filter dimensions** (configurable): `EventType` prefixes, `Domain`, `WorkflowId` allowlists for “learning signal” vs noise.

**Future / secondary sources (design hooks)**

- **Review outcomes:** approving/rejecting queue items could emit dedicated `memory_events` types (not required today) so consolidation treats human decisions as evidence.
- **`memory_items`** (inferred notes): read-only for cross-check until product defines merge with semantics.
- **Explicit profile** is **not** an event stream; consolidation **reads** it via API for **comparison only**, never as merge input into profile writes.

---

## 6. How candidates are generated

**Pipeline (conceptual, aligns with master responsibilities 1–6)**

1. **Normalize** events into typed “signals” (feature extraction): counts, sequences, co-occurrence of `EventType` + `Domain` + `ProjectId`, optional text extraction from `PayloadJson` (schema-validated per `EventType`).
2. **Aggregate per window** into metrics: repetition, recency-weighted frequency, trend deltas (e.g. share of “frontend” vs “backend” tags week-over-week).
3. **Cluster** correlated signals (master: “cluster activity”) — initially **heuristic buckets** (Python); later **embeddings** when pgvector / embedding pipeline exists ([IMPLEMENTATION_PLAN](IMPLEMENTATION_PLAN.md) Phase 2+).
4. **Compare** to existing **`semantic_memories`** (key/domain/claim), **`memory_explicit_profile`** (high-level only: no direct mutation), **`procedural_rules`** (versioned), and **active** episodic highlights already in context ranking ([06-retrieval-engine.md](06-retrieval-engine.md)).
5. **Emit candidates** as a **structured plan**:
   - **Semantic:** new key, updated claim, confidence bump/dip, merge duplicate, archive stale.
   - **Review queue:** `CreateMemoryReviewQueueItem` with `ProposalType` + JSON payloads ([07-review-queue.md](07-review-queue.md)); extend kinds over time (`AdjustConfidence`, `MergeDuplicate` when approval paths exist).
   - **Procedural:** new rule version proposals (Phase 3; consolidation may **queue** only).

**Candidate identity**

- Stable **`proposal_fingerprint`** per run slice (user + normalized key + domain + change kind) to dedupe within a run and across retries.

---

## 7. What can be auto-applied vs what requires approval

Aligned with master **“Auto Apply vs Approval Rules”** and existing backend rules ([08-semantic-memory.md](08-semantic-memory.md) `fromInferredSource`, `InferredOverrideCeiling`).

### 7.1 Safe to auto-apply (low risk, reversible)

Examples from master; map to **concrete API operations** when implemented:

| Action | Mechanism | Guards |
| --- | --- | --- |
| **Increase confidence** on inferred-backed semantic | `PUT …/confidence` with **`fromInferredSource: true`** only if row authority **below** inferred override floor | Never on `AuthorityWeight ≥ UserApprovedSemantic` equivalent. |
| **Attach reinforcing evidence** | `POST …/evidence` with small positive `reinforceConfidenceDelta`, `fromInferredSource: true` | Same authority floor; event must exist for user. |
| **Freshness / last-supported metadata** | Domain fields like `LastSupportedAt` via reinforce path or a future narrow endpoint | No claim text change. |
| **Archive stale weak memory** | `POST …/archive` for rows matching **decay policy** (§9) | Not for user-approved semantics unless policy explicitly allows “archive suggestion” → better as **review** first. |

### 7.2 Requires approval (identity / strategic / structural)

Master examples; route to **`memory_review_queue`**:

- New or **downgraded** **core interest** / long-term **goal** / learning-style defaults (anything that would change **`memory_explicit_profile`** or user-visible “who I am”).
- **New semantic** with **novel key** or **material claim change** vs existing (use `NewSemantic` proposal; approve path already upserts with `UserApprovedSemantic` authority in [07-review-queue.md](07-review-queue.md)).
- **Merge duplicate** semantics across keys/claims when not trivially identical (queue `MergeDuplicate` when approval is implemented).
- **Persistent multi-week trend** that contradicts explicit profile **interpretation** (master conflict example: keep both truths; **queue** resolution narrative, do not auto-edit profile).

### 7.3 Never auto-save (hard stop)

Master **“Never Auto Save”** list — **must not** write to governed stores without human review:

- Sensitive / emotional / speculative **identity** conclusions.
- **Private personal interpretations** not grounded in explicit user statements or stable repeated evidence across windows.
- Any write that would **mutate explicit profile** from consolidation (read-only compare).

---

## 8. Confidence adjustment rules

**Principles**

- **Monotonic caution:** large upward jumps require **stronger evidence** (more events, cross-window confirmation, lower contradiction rate).
- **Contradiction:** new evidence that **conflicts** with claim text or key bucket → **lower confidence** or **queue** clarification (do not oscillate claim text automatically).
- **Recency blend:** weight recent windows higher for “spike vs trend” (yesterday spike alone does not justify 90-day-level confidence).

**Suggested tiers (tunable constants, later data-driven per [IMPLEMENTATION_PLAN Q7](IMPLEMENTATION_PLAN.md#trust-and-policy))**

| Signal strength | Auto delta (inferred path) | Review |
| --- | --- | --- |
| Single-day repetition | Small +δ or none | If contradicts existing semantic |
| 7d sustained pattern | Moderate +δ | If touches profile-adjacent topics |
| 30d + 90d aligned | Larger +δ (still capped) | If new key or merge |

**Existing enforcement:** use **`fromInferredSource: true`** on all consolidation-driven semantic mutations so **`ThrowIfInferredMutationBlocked`** semantics apply ([08-semantic-memory.md](08-semantic-memory.md)).

---

## 9. Memory decay rules

Master: “stale **weak** memories decay”, “contradictory evidence lowers confidence”, “duplicates merge”, “archived inspectable”, “core profile persists until user changes”.

**Semantic memories (`semantic_memories`)**

- **Decay signal:** low `Confidence`, low supporting event rate in 30d/90d, or **negative** reinforcement from contradictory events.
- **Actions:** monotonic confidence decrease (auto if above floor and policy allows), else **queue** “suggested archive” or auto-archive **only** for rows that started as **inferred** and never reached user approval.
- **Archive:** prefer explicit **`MarkArchived`** over delete; keeps inspectability.

**Episodic (`memory_events`)**

- **Append-only** in v1 design; consolidation does **not** delete events. Optional future: **cold storage** / compaction is out of scope here.

**Explicit profile**

- **No decay** from jobs; only user-driven updates.

---

## 10. Duplicate merge rules

**Detection**

- **Relational:** same `UserId` + **case-insensitive** `Key` + **normalized** `Domain` already guarded for Active/Pending ([08-semantic-memory.md](08-semantic-memory.md)).
- **Semantic near-duplicates:** different keys but **high embedding similarity** (future) or same **canonical topic** — produce **`MergeDuplicate`** review items when that proposal type is wired.

**Merge outcomes (when approved)**

- **Single survivor** semantic: merge evidence links, recompute confidence from combined support, **supersede** or **archive** the duplicate row (domain statuses: `Superseded` / `Archived`).
- **Claim text:** prefer **user-approved** or **newer explicit** text; inferred merge must not downgrade user truth.

---

## 11. Python worker ↔ .NET backend communication

**Boundary rule (repo):** workers **do not own** system-of-record state ([workers-platform README](../../workers-platform/README.md)); memory access goes through **`app/memory`** adapters ([IMPLEMENTATION_PLAN](IMPLEMENTATION_PLAN.md) §5).

**Recommended pattern**

| Direction | Mechanism | Notes |
| --- | --- | --- |
| **Python → .NET read** | HTTPS `GET`/`POST` memory APIs: events (paginated), semantics list/find, context packet, explicit profile summary | Pagination and rate limits to avoid thundering herd. |
| **Python → .NET write** | HTTPS commands: ingest event (if consolidation emits meta-events), create/patch review queue, semantic confidence/evidence **with** `fromInferredSource` where applicable | **Auth:** today the API uses **platform access session** after unlock — **not** suitable for headless workers. Implementation plan calls for **worker → API authentication** decision ([IMPLEMENTATION_PLAN Phase 0](IMPLEMENTATION_PLAN.md#phase-0--decisions-and-foundations-prerequisite)). Design expects a **service credential** (API key, mTLS, or internal network + HMAC) scoped to consolidation operations. |
| **.NET → Python** | **Not required** for scheduled nightly flow if Temporal schedule drives Python. Optional: .NET **starts** consolidation via `IWorkflowStarter` for manual “Run now” from admin UI. |

**Contract stability**

- Versioned DTOs under `Platform.Contracts` for any consolidation-specific batch endpoints if high chatter warrants them; until then, compose existing v1 routes.

---

## 12. Observability and run records

**Open decision** ([IMPLEMENTATION_PLAN Q6](IMPLEMENTATION_PLAN.md#temporal-and-consolidation))

| Option | Pros | Cons |
| --- | --- | --- |
| **Reuse `WorkflowRun`** + Temporal workflow id | Already wired from .NET starter | Semantically tied to “product workflow runs”; mixing may confuse dashboards unless `workflow_type` distinguishes consolidation. |
| **Dedicated `memory_consolidation_runs`** (or similar) | Clear audit: windows, counts, errors, per-user stats | New migration + API for ops. |
| **Temporal UI only** | Fastest | Weaker product-level analytics and correlation to memory mutations. |

**Minimum logging (any option)**

- Consolidation run id, `asOf`, user id, window sizes, counts of events read, proposals created, auto-applies, failures, and **idempotency keys** for mutations.

---

## 13. Phasing (implementation alignment)

- **Phase 2** ([IMPLEMENTATION_PLAN](IMPLEMENTATION_PLAN.md)): semantic + review queue + **nightly consolidation** (this doc) + operational story for “Never Auto Save”.
- **Phase 3:** procedural rule proposals from consolidation signals; richer ranking.

---

## 14. Open questions (infrastructure / product)

These are **genuine gaps** after reading the master spec and repo; answers should land in this doc or in [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) before coding.

1. **Authentication:** What is the **stable** worker-to-API auth model (internal API key, OAuth client, mTLS), and does it bypass the browser cookie or use a **machine session** endpoint?
2. **Tenancy:** Consolidation scans **all** `memory_users` vs a configured list vs single-tenant id `1` only?
3. **Schedule:** Exact cron (UTC hour), blackout windows, and **max runtime** before cancel-continue-as-new?
4. **Data-driven policy:** Are auto-apply thresholds **config in DB** vs **constants in Python** for v1 ([IMPLEMENTATION_PLAN Q7](IMPLEMENTATION_PLAN.md#trust-and-policy))?
5. **Emitting meta-events:** Should consolidation writes emit **`memory_events`** (e.g. `memory.consolidation.applied`) for future audit loops—yes/no?
6. **Temporal namespace / task queue:** Stay on default **`platform`** queue with consolidation activities, or isolate to avoid competing with interactive workflows?
7. **pgvector timing:** Does candidate similarity **wait** for embeddings infrastructure, or ship **heuristic-only** consolidation first?

Once these are answered, implementation tickets can split: **(A)** Temporal schedule + workflow skeleton, **(B)** .NET auth + optional batch read APIs, **(C)** Python planner + policy tests, **(D)** observability table + dashboards.
