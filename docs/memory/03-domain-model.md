# Memory system — domain model and application contracts

This document describes the **Memory** bounded context in `backend-platform`: domain entities, value types, invariants, and **Application** ports (abstractions). It aligns with [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) and [backend-standards.md](../backend-platform/docs/backend-standards.md) (wire DTOs in `Platform.Contracts`, domain in `Platform.Domain`, ports in `Platform.Application/Abstractions`).

**Not in this layer:** vector search, Neo4j, LLM/ML scoring, or episodic **persistence** implementation (see Infrastructure and future phases).

---

## Cross-cutting

| Concept | Location | Role |
| --- | --- | --- |
| `MemoryDomainException` | `Platform.Domain/Features/Memory/MemoryDomainException.cs` | Invariant violation (no transport details). |
| `MemoryConflictException` | `.../MemoryConflictException.cs` | Duplicate or conflicting state (e.g. semantic key); mapped to **HTTP 409** in the API host. |
| `MemoryValueConstraints` | `Platform.Domain/Features/Memory/MemoryValueConstraints.cs` | `Clamp01`, `ThrowIfOutOf01` for 0.0–1.0 scores. |
| `AuthorityWeight` | `.../ValueObjects/AuthorityWeight.cs` | 0.0–1.0 authority: e.g. `ExplicitUserTruth` (1.0), `UserApprovedSemantic` / `UserApprovedProcedural` (0.92, review-approved), `Inferred` (0.55). |
| `UncommittedMemoryEvent` | `.../ValueObjects/UncommittedMemoryEvent.cs` | Pre-id episodic append payload; `CreateForIngest` validates. |
| `MemoryUser` + `UserId` | `MemoryUser` entity, `DefaultId` | Single-tenant anchor; all facts scoped by `UserId` (see [02-db-schema.md](02-db-schema.md)). |

---

## Entities (behavior, not anemic DTOs)

### `ExplicitUserProfile`

- **Meaning:** One **user-entered** profile row per `memory_users` id. Stores interests, goals, preferences, active projects, and skill levels in **typed PostgreSQL** columns (`text[]` / `jsonb`). `AuthorityWeight` is always the explicit maximum (1.0) on user updates. **Not** written by inference jobs (see [05-profile-memory.md](05-profile-memory.md)).

### `MemoryItem`

- **Meaning:** Canonical item (profile fact, note, inferred, document) with authority, confidence, importance, freshness, status; optional **`ProjectId`** / **`Domain`** for scoped documents (see [13-document-memory.md](13-document-memory.md)).
- **Factory / actions:** `CreateNew`, `RecordAccess`, `Archive`, `MarkSuperseded`, `PromoteToActive`, `ApplyScoredUpdate`, `SetAuthority`, `IsProfileFact`.
- **Rules (examples):** cannot archive a superseded item; only `Draft` → `Active` via `PromoteToActive`; scores clamped/validated in `0..1` where required.

### `MemoryEvent`

- **Meaning:** Append-only episodic log entry.
- **Factory:** `Create` — requires non-empty `EventType`; `OccurredAt` must not be meaningfully after `CreatedAt` (clock skew window).

### `SemanticMemory`

- **Meaning:** Learned claim with key, domain, confidence, authority, lifecycle.
- **Factory / actions:** `CreateInitial` (optional initial status `Active` / `PendingReview`), `ReinforceWithEvidence` (optional `fromInferredSource` to enforce the inferred-override floor), `SetConfidence`, `SetAuthority`, `ThrowIfInferredMutationBlocked`, `ApplyUserApprovedRevision`, `MarkSuperseded`, `MarkArchived`, `MarkRejected` (from `PendingReview` only).
- **Rules:** reinforce and confidence only when `Active` or `PendingReview`; key and claim required on create. See [08-semantic-memory.md](08-semantic-memory.md) for management rules and API.

### `MemoryEvidence`

- **Meaning:** Link semantic claim ↔ event with strength and optional reason.
- **Factory:** `Link` — valid semantic/event ids, strength in `0..1`.

### `ProceduralRule`

- **Meaning:** Versioned “how the platform should behave” for a workflow.
- **Factory / actions:** `CreateFirstVersion` (requires `AuthorityWeight` in 0..1), `NewVersionWithContent` (version must be previous + 1; optional new authority), `Activate`, `Deprecate`, `SetPriority`, `SetAuthorityWeight`, `SetProvenance`.
- **Rules:** workflow + rule name + provenance `Source` required on first version. `ShouldQueueReviewBeforeApply` uses `ReviewAuthorityFloorForDirectApply` (0.78) unless the caller forces review. See [11-procedural-memory.md](11-procedural-memory.md).

### `MemoryConsolidationRun`

- **Meaning:** One consolidation execution (window bounds, idempotency key, counts, status). Used for ops visibility and **exactly-once** semantics per idempotency key when status is **Completed** (see [10-nightly-worker.md](10-nightly-worker.md)).

### `MemoryReviewQueueItem`

- **Meaning:** Proposal awaiting user decision; carries audit fields (`ApprovedSemanticMemoryId`, `ApprovedProceduralRuleId`, `RejectedReason`, `ResolvedAt`, `ReviewNotes`) after resolution.
- **Factory / actions:** `Propose`, `Approve` (with optional linked semantic id, optional procedural rule id, and notes), `Reject` (with optional reason), `ApplyPendingEdits` (title/summary/proposal JSON while **Pending**), `MarkSuperseded`.
- **Rules:** approve/reject/edit only from `Pending`.

### `MemoryEmbedding`

- **Meaning:** pgvector row anchored to a governed **`MemoryItem`**; long **documents** may have **multiple rows** per model key distinguished by **`ChunkIndex`** (see [13-document-memory.md](13-document-memory.md)).
- **Rules:** unique `(UserId, MemoryItemId, EmbeddingModelKey, ChunkIndex)`; `ContentSha256` for idempotent refresh per row; `Embedding` dimension must match the configured `vector(N)` column.

### `MemoryRelationship`

- **Meaning:** Graph-lite edge (string endpoints + `MemoryRelationshipType`).
- **Factory:** `Define` — non-empty endpoints, no self-loop, type not `Unspecified`, strength in `0..1`.

---

## Application contracts (ports)

All live under `Platform.Application/Abstractions/Memory/`. **Infrastructure** implements; **Api** does not depend on these directly (handlers do).

| Port | File | Purpose |
| --- | --- | --- |
| `IMemoryEventWriter` | `Events/IMemoryEventWriter.cs` | Append `UncommittedMemoryEvent` to episodic store (orchestrated by app; DB in Infrastructure). |
| `IMemoryContextProvider` | `Context/IMemoryContextProvider.cs` + `MemoryContextRequest` | Curated **MemoryContext** v1 packet: SQL + deterministic rank (see [06-retrieval-engine.md](06-retrieval-engine.md)). |
| `IExplicitUserProfileRepository` | `Profile/IExplicitUserProfileRepository.cs` | Read/upsert **explicit** user profile in `memory_explicit_profile` (highest authority; not for inference). See [05-profile-memory.md](05-profile-memory.md). |
| `IMemoryReviewService` | `Review/IMemoryReviewService.cs` | Queue **create**, **list pending**, **patch pending**, **approve** / **reject** (see [07-review-queue.md](07-review-queue.md)). |
| `ISemanticMemoryService` | `Semantic/ISemanticMemoryService.cs` | Create (with initial evidence), list, get-by-id, set confidence, attach evidence, archive, reject, find by key/domain, and find “similar” by substring (see [08-semantic-memory.md](08-semantic-memory.md)). |
| `IProceduralRuleService` | `Procedural/IProceduralRuleService.cs` | Procedural rules: list summaries, detail, create+activate, publish version, priority, activate, deprecate, and apply review-approved proposals (see [11-procedural-memory.md](11-procedural-memory.md)). |
| `IMemoryEmbeddingGenerator` | `Embeddings/IMemoryEmbeddingGenerator.cs` | Query-side embedding for recall (optional; see [12-vector-memory.md](12-vector-memory.md)). |
| `IMemoryVectorRecallSearch` | `Embeddings/IMemoryVectorRecallSearch.cs` | pgvector search joined to `memory_items`. |
| `IMemoryEmbeddingUpsertService` | `Embeddings/IMemoryEmbeddingUpsertService.cs` | Governed upsert into `memory_embeddings` (replaces all chunks for the model key on each run). |
| `IDocumentMemoryIngestService` | `Documents/IDocumentMemoryIngestService.cs` | Persist **`Document`** `memory_items` with metadata and optional embedding index ([13-document-memory.md](13-document-memory.md)). |
| `IMemoryEventsReadRepository` | `Events/IMemoryEventsReadRepository.cs` | Time-window reads over `memory_events` (consolidation and future jobs). |
| `IMemoryEvidenceReadRepository` | `Evidence/IMemoryEvidenceReadRepository.cs` | Existence checks for semantic↔event evidence links. |
| `IMemoryConsolidationRunRepository` | `Consolidation/IMemoryConsolidationRunRepository.cs` | Persist consolidation run rows (`memory_consolidation_runs`). |
| `IMemoryConsolidationPolicyProvider` | `Consolidation/IMemoryConsolidationPolicyProvider.cs` | Thresholds for nightly consolidation (code defaults; swappable). |

**Read-only table ports** (list/detail for HTTP or internal use) remain: `IMemoryItemReadRepository`, `ISemanticMemoryReadRepository`, `IProceduralRuleReadRepository`, `IMemoryReviewQueueReadRepository`, and legacy `ILegacyMemoryInsightsReadRepository`.

**Stubs in Infrastructure** (`*Shell` types in `Stubs/`) return empty results or `NotSupported` for writes where ports are not yet implemented. **`IProceduralRuleReadRepository`** is satisfied by **`EfProceduralRuleService`** together with **`IProceduralRuleService`**.

---

## Design notes

- **No AI in domain:** Scoring, clustering, and ranking belong in later application services or workers; the domain enforces **shape and invariants** only.
- **No vector types** in the domain model; optional future columns can be added with migrations without changing these invariants.
- **Explicit > inferred** is reflected in `AuthorityWeight` and in rules that protect explicit profile items (see `MemoryItem` methods and master “Authority Model”).
- **EF Core** still maps to the same property bags; rich methods are used when **creating** or **mutating** in code paths (handlers, future domain services), not for invalid partial updates from the database without going through the model.

---

## See also

- [02-db-schema.md](02-db-schema.md) — table mapping and indexes
- [05-profile-memory.md](05-profile-memory.md) — explicit profile memory (v1)
- [06-retrieval-engine.md](06-retrieval-engine.md) — GetMemoryContext v1 retrieval
- [07-review-queue.md](07-review-queue.md) — review queue and approval
- [11-procedural-memory.md](11-procedural-memory.md) — procedural rules v1
- [12-vector-memory.md](12-vector-memory.md) — pgvector embeddings and recall
- [13-document-memory.md](13-document-memory.md) — document memory v1
- [MEMORY_MODULE_README.md](MEMORY_MODULE_README.md) — folder layout in the solution
- [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) — phased roadmap
