# Memory system implementation review

This document compares the **current** `backend-platform` memory implementation to the normative design in [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md). For domain details, see also [03-domain-model.md](03-domain-model.md) and [06-retrieval-engine.md](06-retrieval-engine.md).

---

## Follow-up implementation (highest-priority fixes)

The following **P0 / trust / correctness** items from the original prioritized list were implemented in code (details in each section below):

| Theme | Change |
| --- | --- |
| Provenance in context | `SemanticMemoryContextV1Dto` now includes `EvidenceLinkCount`, `SupportingEventIds` (up to 8), and `LastSupportedAt`; `EfMemoryContextProvider` loads evidence joined to events and logs a **warning** when a listed semantic has **zero** evidence rows. |
| Payload trust | `MemoryEventPayloadLimits.MaxPayloadJsonChars` (128 KiB) enforced on ingest; episodic ranking uses only the first **512** UTF-16 characters of JSON (`MemoryEventPayloadForRetrieval`) so huge payloads do not fully enter the ranker. |
| Consolidation trust | `IMemoryConsolidationPolicyProvider.BlocksAutoReinforceForEventType` blocks auto-reinforce for `profile.`, `explicit.`, `preference.`, `goal.`, and `identity.` prefixes; **Information** log when a pattern is skipped for policy. |
| Idempotency / integrity | Unique index `ix_memory_evidence_semantic_event_unique` on `(SemanticMemoryId, EventId)`; `AttachEvidenceAsync` returns early when the link already exists (idempotent attach). |
| Transactions | `CreateWithInitialEvidenceAsync` wraps semantic + evidence inserts in a transaction with **rollback** on any failure after the transaction starts. |
| Tests | Unit tests for payload limits, retrieval truncation, consolidation policy; integration tests for context provenance, duplicate attach idempotency, and blocked consolidation reinforce. |

**Still open** (not in this pass): full PII redaction pipeline, structured per-`EventType` payload schemas, rich consolidation policy matrix, graph in context, decay/merge, audit log table, explicit-vs-semantic conflict objects, and alignment of scoring weights with the master doc.

---

## Executive summary

The codebase delivers a **credible Phase 1–2 slice**: typed PostgreSQL as source of truth, episodic events, governed semantics with evidence links and an inferred-mutation **floor** (`AuthorityWeight.InferredOverrideCeiling`), a **review queue** and **nightly consolidation** (service-token–protected), **GetMemoryContext** with deterministic ranking and optional **pgvector** recall on `memory_items`, and explicit profile memory with strong domain documentation.

Gaps vs the master architecture remain material for a 10-year product: **no graph in context**, **no working-memory layer**, **no decay/merge engine** as specified, **limited conflict modeling** (explicit vs inferred is mostly implicit), **audit logging** is not present, and **retrieval math** only partially matches the document’s suggested formula. **Multi-user** is intentionally constrained to a single tenant id in several validators. **Context** now surfaces basic **evidence provenance** for semantics; episodic payloads are **bounded** on ingest and **capped** for ranking.

---

## 1. Explicit memory authority

**Master intent:** Explicit user-entered truth = highest authority (e.g. 1.0); do not overwrite with inference; represent both.

**Implementation:**

- `ExplicitUserProfile` uses `ApplyUserUpdate` and resets `AuthorityWeight` to `ExplicitUserProfileContent.ExplicitUserAuthorityValue` (1.0). Class documentation states inference must not call `ApplyUserUpdate`.
- `MemoryItem` and `SemanticMemory` carry their own `AuthorityWeight` / confidence; high-authority semantics block inferred mutations via `ThrowIfInferredMutationBlocked` on `SemanticMemory`.
- **Gap:** There is no automated **cross-check** in `GetMemoryContext` that labels an inferred semantic as “contradicting” a specific explicit profile line (e.g. core interest). Resolution is **left to ranking** (explicit facts score with a high floor) and human review, not a first-class “conflict” between profile and semantic.

**Verdict:** Strong for **write-path** protection; **read-path / conflict narrative** is thinner than the master’s “track both truths” example.

---

## 2. Inferred memory evidence

**Master intent:** Inferred memories require evidence; user can see why a memory exists.

**Implementation:**

- `ISemanticMemoryService.CreateWithInitialEvidenceAsync` requires an `eventId` and creates a `MemoryEvidence` row in the same transaction.
- `AttachEvidenceAsync` links events to semantics and optionally reinforces confidence; `EfMemoryEvidenceReadRepository` supports existence checks.
- **Gaps (remaining):**
  - **Context packet:** provenance is now **counts + supporting event ids + last supported** on each semantic in `GetMemoryContext`; there is still no embedded **evidence reason** or full timeline in that DTO.
  - **Orphan semantics:** partial unique index on active/pending `(UserId, lower(Key), coalesce(Domain,''))` already exists (`SemanticMemoryDedupIndexV1`); evidence uniqueness is now enforced at DB level for `(SemanticMemoryId, EventId)`. A semantic with **zero** evidence remains possible only if data is written outside the application services—**warned** in logs when read for context.

**Verdict:** Evidence is **modeled and enforced in the write service**; **observability in the main context API** is materially improved; deep “inspect why” (reasons, full evidence payloads) remains future work.

---

## 3. Approval boundaries

**Master intent:** Auto-apply low-risk reversible changes; require approval for identity/strategic changes; never auto-save sensitive conclusions.

**Implementation:**

- Nightly consolidation **reinforces** existing inferred-level semantics (below the override ceiling) and **queues** new semantics as review items; duplicate proposal fingerprints are skipped.
- `ProceduralRule.ShouldQueueReviewBeforeApply` encodes a review floor for **procedural** changes.
- **Gaps (remaining):**
  - Consolidation proposals are still driven by **repeated event types**, not a rich policy taxonomy (e.g. “new core interest” vs “ephemeral spike”).
  - **Auto-apply** of confidence via reinforcement is bounded by `ReinforceConfidenceDelta` and the inferred floor; **prefix-based** blocking (`profile.`, `explicit.`, etc.) stops automated reinforcement for identity-like event families—**not** a full sensitive-claim detector.

**Verdict:** **Stronger v1 guardrails** for trust-sensitive episodic families; not yet the **policy matrix** implied by the master doc.

---

## 4. Accidental direct writes

**Master intent:** Agents use Memory API; emit events; do not self-write long-term memory.

**Implementation:**

- User-facing routes go through **session** middleware (`RequirePlatformAccessMiddleware`); internal consolidation uses a **Bearer service token** (`InternalMemoryWorkerAuthenticationMiddleware`).
- Domain updates for profile go through `UpdateProfileMemoryCommandHandler` and `ExplicitUserProfile` invariants.
- **Residual risks:**
  - Any code with `PlatformDbContext` and credentials can still **insert** rows (ORM escape hatch). There is no row-level or DB-level “append-only only for events” enforcement.
  - `Platform.DevData` and tests can mass-write; acceptable for dev, but worth noting for production governance.

**Verdict:** **API surface** is controlled; **deeper enforcement** (DB policies, separate roles, or append-only `memory_events` grants) is out of scope today.

---

## 5. Privacy risks

**Master intent:** Trust and privacy are mandatory; reduce “creepiness.”

**Implementation:**

- `GetMemoryContext` **truncates** episode payloads for display (`TruncatePayload`); still uses `PayloadJson` in ranking (`TextMatchRatio`), so **semantic content** of events may influence scores.
- Event ingestion validates JSON, field lengths, and a **128 KiB** cap on `PayloadJson` (`MemoryEventPayloadLimits`); **no explicit PII redaction** pipeline. Ranking uses only a **512-character** prefix of payload JSON for text match.
- **Single-tenant** assumption (`UserId` must be 0/1 in `IngestMemoryEventCommandValidator`) reduces cross-user risk but is not a substitute for future multi-tenant isolation testing.

**Verdict:** **Basic** safeguards; no **data classification**, **retention policy**, or **redaction** pipeline as described for a long-lived personal system.

---

## 6. Duplicate memories

**Master intent:** Deduplicate and merge; keep inspectable history.

**Implementation:**

- `CreateWithInitialEvidenceAsync` conflicts if an active/pending row exists for `(user, key, domain)`.
- `EfMemoryContextProvider` adds **conflicts** when the **same normalized semantic key** appears with **more than one distinct claim** (duplicate key / divergent claims).
- **Gaps (remaining):** No **merge** workflow, no **nightly merge** of near-duplicate keys across domains. Active/pending semantic dedup is enforced by partial unique index `ix_semantic_memories_user_key_domain_active_pending`; **evidence** rows are now unique per `(SemanticMemoryId, EventId)`.

**Verdict:** **Detection** plus **stronger integrity** on evidence links; **remediation** (merge) is not implemented as in the master roadmap.

---

## 7. Stale memory handling

**Master intent:** Decay, contradiction lowering confidence, merge duplicates, archive, core profile stable until user changes.

**Implementation:**

- `SemanticMemory` supports archive/reject/supersede; `MemoryItem` has lifecycle and scoring hooks.
- Nightly job **reinforces** and **proposes**; it does **not** implement time-based **decay**, **contradiction** counters (beyond reinforcement deltas), or **stale cleanup** proposals in the sense of the master document.
- **Freshness** on `MemoryItem` exists in the model; **systematic** decay in consolidation is not evident in `ExecuteNightlyMemoryConsolidationCommandHandler`.

**Verdict:** **Largely unimplemented** vs the “evolution and decay” section; acceptable as Phase 1–2, but a **major** gap for long-horizon behavior.

---

## 8. Retrieval quality

**Master intent:** Curated `GetMemoryContext`, selective relevance, suggested composite scoring.

**Implementation:**

- `EfMemoryContextProvider` assembles profile facts, goals, projects, semantics, episodes, procedural rules, optional **vector** hits; caps list sizes; uses `MemoryContextV1Scoring` (authority, confidence, recency, workflow/project fit, text match, domain, status).
- The master’s example formula (authority 0.35, similarity 0.25, recency 0.15, confidence 0.15, workflow 0.10) differs from **implemented** weight constants (e.g. authority 0.38, and text match embedded in a combined rank rather than a standalone “similarity” term as documented).
- Episodic examples use a **fixed neutral authority** (0.55) and a constant “confidence” for ranking—not derived from event semantics.
- `memory_relationships` (graph metadata) is **not** included in the context DTO, though `MemoryModuleDescriptorV1Dto` advertises a graph area.

**Verdict:** **Useful v1** deterministic ranking; **not** yet aligned 1:1 with the **documented** scoring recipe or the **full** retrieval pipeline (no graph, limited episodic signal).

---

## 9. Test coverage

**Existing automated tests (representative):**

- **Unit:** `MemoryContextV1ScoringTests`, `SemanticMemoryDomainTests`, `MemoryReviewQueueItemDomainTests`, `ProceduralRuleDomainTests`, `UpdateProfileMemoryCommandValidatorTests`, `IngestMemoryEventCommandValidatorTests`, `MemoryConsolidationKeysTests`, `DeterministicRecallEmbeddingGeneratorTests`, `MemoryEmbeddingCanonicalTextTests`, `DocumentMemoryChunkBuilderTests`, `CreateReviewQueueItemCommandValidatorTests`.
- **Integration:** `MemoryContextV1FlowTests`, `MemoryEventIngestionFlowTests`, `DocumentMemoryV1FlowTests`, `MemoryVectorRecallV1FlowTests`, `SemanticMemoryV1FlowTests`, `MemoryReviewV1FlowTests`, `ProceduralMemoryV1FlowTests`, `ProfileMemoryV1FlowTests`, `MemoryConsolidationInternalV1Tests` (ephemeral DB via Testcontainers for the memory collection).

**Gaps:**

- No **concurrency** test proving uniqueness under parallel creates for the same semantic key.
- **End-to-end** “master scenario” (explicit spike + inferred semantic + conflict resolution) is not clearly isolated as one scenario test.
- **Security:** limited automated proof that **internal** routes reject bad tokens in integration (may exist partially—worth extending).

**Verdict:** **Solid** relative to a young module; **thin** on policy, privacy, and decay paths.

---

## 10. Naming consistency

**Observations:**

- Table names mix styles: e.g. `MemoryInsights` / `NewsItems` (Pascal) vs `memory_items`, `semantic_memories` (snake, lowercase) per `MemoryV1EfConfiguration`.
- DTOs use `SemanticMemory*`, `EpisodicExample*`, `MemoryItemVectorRecall*`; domain uses `MemoryItem`, `MemoryEvent`—generally consistent but **“Example”** vs “episode” in prose can confuse.
- **Legacy** `MemoryInsight` remains a separate table (`LEGACY_MEMORY_REMOVAL.md` and domain comments).

**Verdict:** **Acceptable** but **not uniform**; migration to one naming scheme would help operators and new contributors.

---

## 11. Architecture violations / drift

| Area | Master | Current implementation |
| --- | --- | --- |
| Working memory | Ephemeral layer | **Not** modeled as a distinct store/API in backend |
| Graph memory | Relationships for multi-hop | `memory_relationships` exists; **not** in `GetMemoryContext` output |
| Audit / inspectability | Audit logs in PG | **No** first-class audit trail for memory mutations |
| Agent rule | “Never direct write LT memory” | **Convention** + API; not DB-enforced |
| Consolidation | Multi-window (7/30/90d), merge, decay | **Single** overnight window in handler; no merge/stale pass |
| Storage | Blobs for large docs | **Chunked** `MemoryItem` + embeddings; full blob store **not** central in review scope |

**Verdict:** Several items are **documented as roadmap** rather than violations, but the **graph** and **audit** gaps are visible **omissions** relative to “non-negotiable” inspectability and trust as the system matures.

---

## Prioritized fixes (status)

**P0 — trust and safety**

1. ~~Expose evidence/provenance in read APIs~~ **Done (v1 slice):** counts, supporting event ids, `LastSupportedAt`, orphan warning in logs.
2. **Harden PII/secret handling** — **Partially done:** max payload size + capped payload for ranking; **remaining:** redaction, per-`EventType` schemas, retention.
3. **Consolidation policy** — **Partially done:** prefix-based `BlocksAutoReinforceForEventType` on `IMemoryConsolidationPolicyProvider`; **remaining:** broader policy matrix / claim-shape heuristics.

**P1 — data integrity and long-term behavior**

4. ~~Unique constraint for active/pending semantics~~ **Already present** (`SemanticMemoryDedupIndexV1`); **added** unique `(SemanticMemoryId, EventId)` on `memory_evidence`.
5. **Stale / decay** — **Open.**
6. **Duplicate merge** — **Open.**
7. **Graph in context** — **Open.**

**P2 — product polish and alignment**

8. **Align scoring** with master or update master doc — **Open.**
9. **End-to-end explicit vs inferred conflict narrative test** — **Open** (partial trust coverage added via consolidation block test).
10. **Legacy `MemoryInsights`** — **Open.**
11. **Naming cleanup** — **Open.**
12. **Audit log** — **Open.**

---

*Line-level references are indicative—re-verify when implementing further changes.*
