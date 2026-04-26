# Memory review queue and approval (v1)

## Purpose

The system may **propose** memory changes (e.g. new semantic claims). **Important** changes must not be applied until a **human approves** them. The `memory_review_queue` table holds proposals; **approve** applies them (for supported types); **reject** records a decision **without** changing governed memory (beyond queue status).

## Data model

Table **`memory_review_queue`** (see [02-db-schema.md](02-db-schema.md)). v1 adds audit / resolution columns (migration **`MemoryReviewQueueAuditV1`**):

| Column | Role |
| --- | --- |
| `ApprovedSemanticMemoryId` | When a **NewSemantic** proposal is approved, FK to the created or updated `semantic_memories` row. |
| `ApprovedProceduralRuleId` | When a **NewProceduralRule** proposal is approved, FK to the created or updated `procedural_rules` row. |
| `RejectedReason` | Optional text when **Rejected**. |
| `ResolvedAt` | Timestamp when status became **Approved** or **Rejected**. |
| `ReviewNotes` | Optional free-text notes on **approve** (audit). |

Existing columns: `ProposalType`, `Title`, `Summary`, `ProposedChangeJson` (jsonb), `EvidenceJson` (jsonb), `Priority`, `Status`, `CreatedAt`, `UpdatedAt`.

## Status transitions

| From | Action | To |
| --- | --- | --- |
| `Pending` | **Approve** | `Approved` (+ semantic upsert for `NewSemantic`, or procedural rule create/version for `NewProceduralRule`) |
| `Pending` | **Reject** | `Rejected` (no semantic write) |
| `Pending` | **PATCH** (edit) | `Pending` (updates fields only) |
| `Pending` | (internal) **MarkSuperseded** | `Superseded` (existing domain helper) |

Only **Pending** items may be approved, rejected, or edited.

## Proposal payloads

### `NewSemantic` (`MemoryReviewProposalType.NewSemantic`)

`ProposedChangeJson` must be JSON:

```json
{ "kind": "NewSemantic", "key": "stable-key", "claim": "text", "domain": null, "initialConfidence": 0.65 }
```

- **Approve:** finds an **Active** or **PendingReview** semantic with the same **user** + **key** (case-insensitive); if found, applies **`ApplyUserApprovedRevision`** with authority **`AuthorityWeight.UserApprovedSemantic` (0.92)** — **higher than** unconfirmed inferred material (`AuthorityWeight.Inferred` 0.55) but below direct explicit user truth (1.0). If none exists, **creates** a new `SemanticMemory` with the same authority.
- **Reject:** queue row only; **no** semantic row is created or updated.

### `NewProceduralRule` (`MemoryReviewProposalType.NewProceduralRule`)

`ProposedChangeJson` must be JSON:

```json
{ "kind": "NewProceduralRule", "workflowType": "learning", "ruleName": "session_style", "ruleContent": "…", "priority": 0, "source": "inferred:worker", "authorityWeight": 0.55, "basisRuleId": null }
```

- **`basisRuleId` null:** first version for `(workflowType, ruleName)`; `workflowType`, `ruleName`, `ruleContent`, and `source` are required.
- **`basisRuleId` set:** new **version** for the rule family of that row; `ruleContent` and `source` are required (workflow/name are taken from stored rule versions on approve).

**Approve:** creates or versions a `procedural_rules` row with authority **`UserApprovedProcedural` (0.92)**, deprecates prior **Active** rows for the same `(UserId, WorkflowType, RuleName)`, and activates the new row.

Other proposal kinds (`AdjustConfidence`, `MergeDuplicate`) are **not** implemented for approval in v1 and return a domain error if approved.

## HTTP (v1)

Requires an **unlocked** platform session.

| Method | Path | Body | Result |
| --- | --- | --- | --- |
| `GET` | `/api/v1/memory/review-queue?userId=` | — | `MemoryReviewQueueItemV1Dto[]` (**Pending** only) |
| `POST` | `/api/v1/memory/review-queue` | `CreateMemoryReviewQueueItemV1Request` | Created item DTO |
| `PATCH` | `/api/v1/memory/review-queue/{id}?userId=` | `PatchMemoryReviewQueueItemV1Request` (≥ one field) | Updated pending item DTO |
| `POST` | `/api/v1/memory/review-queue/{id}/approve?userId=` | `ApproveMemoryReviewQueueItemV1Request` (optional `reviewNotes`) | `ApproveMemoryReviewQueueItemV1Response` (`semanticMemoryId` and/or `proceduralRuleId`) |
| `POST` | `/api/v1/memory/review-queue/{id}/reject?userId=` | `RejectMemoryReviewQueueItemV1Request` (optional `reason`) | **204** |

`userId` optional; `0`/omitted → default `memory_users` id `1`.

## Application layer

- **Create:** `CreateReviewQueueItemCommand` + `CreateReviewQueueItemCommandHandler` + `CreateReviewQueueItemCommandValidator`
- **List:** `ListMemoryReviewQueueQuery` + `ListMemoryReviewQueueQueryHandler` (uses `IMemoryReviewService.ListPendingAsync`)
- **Patch:** `PatchReviewQueueItemCommand` + handler + validator
- **Approve / reject:** dedicated handlers calling **`IMemoryReviewService`** (`EfMemoryReviewService` — transactional approve for `NewSemantic` and `NewProceduralRule`)

## See also

- [03-domain-model.md](03-domain-model.md) — `MemoryReviewQueueItem`, `SemanticMemory`
- [06-retrieval-engine.md](06-retrieval-engine.md) — context packet (may surface pending semantics)
- [11-procedural-memory.md](11-procedural-memory.md) — procedural rules and review payloads
