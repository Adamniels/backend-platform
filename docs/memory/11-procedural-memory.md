# Procedural memory v1

## Purpose

**Procedural memory** stores **behavior rules** for how agents and workflows should operate for a user: ranking preferences, explanation style, learning-session generation constraints, workflow-specific personalization, and similar **prescriptive** text. It complements **semantic** memory (factual claims), **profile** memory (explicit interests and goals), and **episodic** events.

Implementation lives in `backend-platform` (domain, application handlers, EF, HTTP v1 routes).

---

## Data model

Table **`procedural_rules`** (see [02-db-schema.md](02-db-schema.md)). Each row is one **version** of a rule scoped by `(UserId, WorkflowType, RuleName, Version)` (unique index).

| Field | Role |
| --- | --- |
| `WorkflowType` | Logical workflow key (e.g. `learning`, `recommendation`) used for relevance in `GetMemoryContext`. |
| `RuleName` | Stable name within the workflow (e.g. `session_generation`, `explanation_style`). |
| `RuleContent` | Rule body (markdown or structured text). |
| `Priority` | Integer tie-breaker and rank boost in context assembly. |
| `Source` | **Provenance** string (required); e.g. `user:settings`, `inferred:worker`. |
| `AuthorityWeight` | **0.0–1.0**; used in `GetMemoryContext` ranking (same scale as other memory authority). |
| `Version` | Monotonic per rule identity; only **one active** row per `(UserId, WorkflowType, RuleName)` at a time. |
| `Status` | `Inactive`, `Active`, `Deprecated` (`ProceduralRuleStatus`). |

Factories and transitions: `CreateFirstVersion`, `NewVersionWithContent` (version must be previous + 1), `Activate`, `Deprecate`, `SetPriority`, `SetAuthorityWeight`, `SetProvenance`. See `Platform.Domain/Features/Memory/Entities/ProceduralRule.cs`.

---

## Versioning and review

- **Version-ready:** new content is always a **new row** with `Version + 1`; older rows remain for audit.
- **Review gate:** if `AuthorityWeight` is **below** `ProceduralRule.ReviewAuthorityFloorForDirectApply` (**0.78**), or the client sets **`ForceSubmitForReview`**, the API **does not** write an active rule immediately. It enqueues **`memory_review_queue`** with proposal type **`NewProceduralRule`** and JSON payload kind **`NewProceduralRule`** (see [07-review-queue.md](07-review-queue.md)).
- **After approval:** authority is elevated to **`AuthorityWeight.UserApprovedProcedural` (0.92)** and the rule is **activated** (prior active versions for the same identity are **deprecated**).

---

## HTTP (v1)

Requires an **unlocked** platform session (same pattern as other memory v1 routes).

| Method | Path | Body | Result |
| --- | --- | --- | --- |
| `GET` | `/api/v1/memory/procedural-rules?userId=` | — | `ProceduralRuleSummaryV1Dto[]` |
| `GET` | `/api/v1/memory/procedural-rules/{id}?userId=` | — | `ProceduralRuleDetailV1Dto` or **404** |
| `POST` | `/api/v1/memory/procedural-rules` | `CreateProceduralRuleV1Request` | **201** `CreateProceduralRuleV1Response` (`Outcome`: `Activated` or `PendingReview`) |
| `POST` | `/api/v1/memory/procedural-rules/{id}/versions` | `PublishProceduralRuleVersionV1Request` | **201** `PublishProceduralRuleVersionV1Response` |
| `PUT` | `/api/v1/memory/procedural-rules/{id}/priority` | `UpdateProceduralRulePriorityV1Request` | `ProceduralRuleDetailV1Dto` or **404** |
| `POST` | `/api/v1/memory/procedural-rules/{id}/activate` | — | `ProceduralRuleDetailV1Dto` (deprecates other active rows for the same rule identity) |
| `POST` | `/api/v1/memory/procedural-rules/{id}/deprecate` | — | `ProceduralRuleDetailV1Dto` |

`userId` optional; `0`/omitted → default `memory_users` id `1`.

---

## Retrieval (`GetMemoryContext`)

`EfMemoryContextProvider` loads **active** procedural rules, keeps the **latest `Version`** per `(WorkflowType, RuleName)`, and ranks them using each row’s **`AuthorityWeight`** (no longer a hard-coded constant). DTOs include **`Source`** and **`AuthorityWeight`** on `ProceduralRuleContextV1Dto`. See [06-retrieval-engine.md](06-retrieval-engine.md).

---

## Application and infrastructure

| Component | Location |
| --- | --- |
| Port | `IProceduralRuleService` extends `IProceduralRuleReadRepository` — `Platform.Application/Abstractions/Memory/Procedural/` |
| EF implementation | `EfProceduralRuleService` — `Platform.Infrastructure/Features/Memory/Procedural/` |
| Review approval | `EfMemoryReviewService` — `NewProceduralRule` branch calls `ApplyApprovedNewProceduralProposalAsync` |
| Proposal JSON helpers | `MemoryReviewProposalJson` — `Platform.Application/Features/Memory/Review/` |
| Routes | `ProceduralMemoryV1Routes` — `Platform.Api/Features/Memory/Procedural/` |

Migration: **`ProceduralMemoryAuthorityV1`** adds `AuthorityWeight` on `procedural_rules` and `ApprovedProceduralRuleId` on `memory_review_queue`.

---

## See also

- [02-db-schema.md](02-db-schema.md) — column list and indexes
- [03-domain-model.md](03-domain-model.md) — entities and ports
- [06-retrieval-engine.md](06-retrieval-engine.md) — context assembly
- [07-review-queue.md](07-review-queue.md) — approval payloads and behavior
- [MEMORY_MODULE_README.md](MEMORY_MODULE_README.md) — doc index
