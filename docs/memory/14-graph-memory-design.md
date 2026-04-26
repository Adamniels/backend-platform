# Graph memory — design (Neo4j not implemented)

This document specifies a **future** graph layer for the Memory bounded context. **Neo4j is not implemented** today; relational **`memory_relationships`** and SQL remain the only persisted edges ([02-db-schema.md](02-db-schema.md), [03-domain-model.md](03-domain-model.md)).

**Non-negotiable:** **PostgreSQL is the source of truth.** A graph database (Neo4j or similar) is a **relationship projection and reasoning layer** only: optimized for multi-hop traversal, path scoring, and exploratory queries. It must never become the system of record for facts, claims, or lifecycle.

---

## Goals

- Support **multi-hop** and **path-centric** questions (“what connects this project to these semantics?”, “which documents evidence this claim?”) without heavy recursive SQL.
- Keep **governed writes** on PostgreSQL; graph receives **derived** nodes and edges.
- Integrate results into **`GetMemoryContext`** as an **optional, ranked slice** with clear provenance and staleness handling ([06-retrieval-engine.md](06-retrieval-engine.md)).

---

## Entities (conceptual)

### Owned by PostgreSQL (authoritative)

| Concept | Tables / rows (today or planned) | Role |
| --- | --- | --- |
| User tenancy | `memory_users`, `UserId` on all rows | Partitioning and FK integrity. |
| Canonical memory items | `memory_items` | Facts, notes, documents, etc.; lifecycle and authority. |
| Semantic claims | `semantic_memories` | Approved / pending truth; review workflow. |
| Evidence links | `memory_evidence`, episodic `memory_events` | Provenance between claims and events. |
| Relational edges (graph-lite) | `memory_relationships` | **Authoritative edge declarations** in v1 (string endpoints + type + strength); future graph **materializes** from these and other rules. |
| Embeddings | `memory_embeddings` | Retrieval projection from items; not truth. |

### Projected in the graph (non-authoritative)

Graph **nodes** are projections keyed by stable identities that PostgreSQL defines (e.g. `SemanticMemory:{id}`, `MemoryItem:{id}`, `Project:{externalId}`, `Concept:{normalizedKey}` if ever introduced). Graph **relationships** mirror or enrich PG-backed edges:

- **Mirror edges:** one-to-one mapping from `memory_relationships` (or equivalent derived links) into typed Neo4j relationships with the same semantic predicate.
- **Derived edges:** computed in workers from rules (e.g. “document chunk **supports** semantic key”, “event **triggers** review item”) **only** when the derivation rule is traceable to PG rows or explicit configuration.

The graph may add **ephemeral** or **session-scoped** nodes for reasoning (e.g. query anchors); those must not be written back as truth without a PG transaction.

---

## Relationships (graph model)

### Core relationship families (illustrative)

| Relationship (logical) | From (example label) | To (example label) | Source in PG |
| --- | --- | --- | --- |
| `RELATES_TO` | `MemoryItem` | `MemoryItem` | Inferred or user-declared links materialized into `memory_relationships` or future normalized edge table. |
| `EVIDENCES` | `MemoryItem` (document) | `SemanticMemory` | Derived from `memory_evidence` + item type rules ([08-semantic-memory.md](08-semantic-memory.md)). |
| `MENTIONS_PROJECT` | `MemoryItem` | `Project` | `memory_items.ProjectId` or profile project JSON ([13-document-memory.md](13-document-memory.md)). |
| `TAGGED_DOMAIN` | `SemanticMemory` / `MemoryItem` | `Domain` | `domain` columns / enums. |
| `SAME_AS` / `DUPLICATE_OF` | `SemanticMemory` | `SemanticMemory` | Review / dedup pipeline; must remain consistent with PG constraints. |

**Cardinality:** many-to-many is normal in the graph; PostgreSQL continues to enforce uniqueness and status rules (e.g. semantic key per user) in SQL.

**Identity mapping:** every graph node that represents durable platform state carries `userId` and a **stable PG pointer** (`table`, `id` or natural key agreed in schema). Ad-hoc graph-only IDs are not authoritative.

---

## What PostgreSQL owns

- **All writes that change truth or lifecycle:** create/update/archive/reject semantics, promote memory items, ingest documents, procedural changes, review resolutions.
- **Schema migrations, constraints, and audit trails** for governed memory.
- **Idempotency keys** for any job that pushes to the graph (same pattern as consolidation runs, [10-nightly-worker.md](10-nightly-worker.md)).
- **The decision** whether an edge exists at all: graph sync is **downstream**; if PG deletes or supersedes a row, the graph must eventually reflect **absence** or **tombstone**.

---

## What the graph database projects

- **Fast traversal** across many hops with predictable cost (indexed relationships, path variables).
- **Path scoring** that combines relationship strength, hop count, and optional weights (e.g. boost user-approved semantics).
- **Exploratory subgraphs** for agents (“expand neighborhood of this semantic id to depth 2”).
- **Optional denormalization** for read performance (e.g. cached labels on nodes) as long as it can be rebuilt from PG.

The graph does **not** own: final authority scores, review state, or “what is true” for the user—those remain in PostgreSQL.

---

## Sync strategy: PostgreSQL → graph

### Principles

1. **Asynchronous, at-least-once** delivery from committed PG transactions to graph upserts (Temporal workflow, queue consumer, or outbox table—pick one implementation later; design assumes **outbox or change capture + worker**).
2. **Idempotent graph writes:** primary key in graph = `(userId, sourceTable, sourceId, edgeKind, targetTable, targetId)` or a hash thereof; retries must not duplicate logical edges.
3. **Ordering per aggregate:** per `UserId` + entity id, apply events in monotonic order (version or `UpdatedAt` from PG).
4. **Tombstones / deletes:** PG hard-delete or status change emits **delete** or **archived** event; graph worker removes or marks inactive edges/nodes so recall does not resurface archived content.

### Suggested stages (logical)

| Stage | Behavior |
| --- | --- |
| **Capture** | After successful PG `SaveChanges`, append **GraphSyncCommand** (outbox row) with payload: operation, entity refs, correlation id. |
| **Project** | Worker reads outbox, loads any extra PG fields needed for labels, executes **MERGE**-style upserts in Neo4j. |
| **Verify** | Periodic reconciliation job: sample PG edges vs graph counts; emit metrics and alerts on drift. |

### Failure modes

- If graph is down, **PG remains fully usable**; sync backlog grows; `GetMemoryContext` may omit graph slice or attach a warning (see below).
- **Never** block user-facing PG commits on graph success.

---

## Relationship strength

### Inputs (from PostgreSQL or derived rules)

- **`memory_relationships.Strength`** (0–1) when the edge originates there.
- **Authority** from attached entities (`semantic_memories.AuthorityWeight`, document evidence band per [13-document-memory.md](13-document-memory.md)).
- **Recency** from `UpdatedAt` / `OccurredAt` on source rows.

### Projection rule (design-level)

- Persist on the graph relationship a **`projectedStrength`** in `[0,1]` computed as a documented function, e.g. `clamp01( w1 * edgeStrength + w2 * min(sourceAuthority, targetAuthority) + w3 * recencyFactor )`.
- **Path aggregate strength** for `GetMemoryContext`: combine hop strengths with a decay on distance (e.g. multiply by `γ^(hop-1)`), and cap paths that include low-authority or `PendingReview` semantics unless explicitly allowed by policy.

Exact weights are a product decision when implemented; they belong in configuration, not hard-coded in Neo4j alone.

---

## Example queries (illustrative Cypher-style)

These are **design examples**; syntax may vary by Neo4j version.

**1. Two-hop neighborhood around a semantic memory (approved/active only in PG; graph mirrors status on node props).**

```cypher
MATCH (s:SemanticMemory { userId: $userId, pgId: $semanticId })
MATCH p = (s)-[*1..2]-(n)
WHERE ALL(r IN relationships(p) WHERE r.projectedStrength >= $minStrength)
RETURN p
LIMIT 50
```

**2. Documents that evidence a claim (via projected `EVIDENCES` edges).**

```cypher
MATCH (d:MemoryItem:Document { userId: $userId })-[e:EVIDENCES]->(s:SemanticMemory { userId: $userId, pgId: $semanticId })
RETURN d.pgId AS memoryItemId, e.projectedStrength AS strength
ORDER BY strength DESC
LIMIT 16
```

**3. Shortest path between two memory items (bounded cost).**

```cypher
MATCH (a:MemoryItem { userId: $userId, pgId: $fromId })
MATCH (b:MemoryItem { userId: $userId, pgId: $toId })
MATCH p = shortestPath((a)-[*..6]-(b))
WHERE ALL(r IN relationships(p) WHERE r.projectedStrength >= $minStrength)
RETURN p
LIMIT 5
```

**4. Projects linked to a set of semantics through documents (3-hop pattern).**

```cypher
MATCH (s:SemanticMemory { userId: $userId })
WHERE s.pgId IN $semanticIds
MATCH (s)<-[:EVIDENCES]-(d:MemoryItem:Document)-[:MENTIONS_PROJECT]->(p:Project)
RETURN DISTINCT p.externalId AS projectId, count(*) AS weight
ORDER BY weight DESC
LIMIT 8
```

---

## How graph results enter `GetMemoryContext`

### Contract shape (future)

- Add an optional slice, e.g. **`graphNeighborhoods`** or **`graphPaths`**, each entry containing:
  - **Summary** (human-readable one-liner).
  - **Anchors:** PG ids and types for every node in the path (for audit and UI deep links).
  - **Aggregate score** (path strength after decay).
  - **`provenance`:** `"graph_projection"` plus **sync watermark** or **graph query version** so clients know this is not SQL-only.

### Assembly rules

- **Never** elevate graph-only paths to semantic truth; they appear as **context / hypotheses**.
- If **`includeGraphRecall`** (name TBD) is false or graph client unavailable: omit slice; `assemblyStage` might remain `v1-sql+vector` or gain `+graph` suffix when graph contributed.
- If graph results are **stale** (watermark older than PG `UpdatedAt` for any anchor): add a **`MemoryWarningV1Dto`** (e.g. `code: "graph_recall_stale"`).

### Placement relative to other slices

- Run **after** SQL-ranked slices and **alongside** vector recall; merge scores in application code with **explicit caps** (e.g. max N paths, max depth D) to protect latency.

---

## Migration path: no graph → graph

| Phase | PostgreSQL | Graph | `GetMemoryContext` |
| --- | --- | --- | --- |
| **0 — Today** | Full truth; `memory_relationships` optional, SQL-only expansion. | None. | SQL + optional vector ([12-vector-memory.md](12-vector-memory.md)). |
| **1 — Dual-write, read SQL only** | Same; every edge write also enqueues sync. | Materialize nodes/edges; backfill from PG. | Unchanged; monitor sync lag. |
| **2 — Shadow read** | Same. | Graph queried in parallel; results logged/diffed, not returned to clients. | Optional internal flag for experiments. |
| **3 — Augmented context** | Same. | Graph trusted for **relationship** recall only. | New optional slice + warnings; defaults off until stable. |
| **4 — Optimize** | Still SoT. | Indexes, derived edges, denormalized labels tuned for hot queries. | Tune budgets and ranking. |

Rollback: disable graph reads; drain or drop Neo4j data; PostgreSQL unchanged.

---

## See also

- [02-db-schema.md](02-db-schema.md) — `memory_relationships` today
- [03-domain-model.md](03-domain-model.md) — `MemoryRelationship` entity
- [06-retrieval-engine.md](06-retrieval-engine.md) — `GetMemoryContext` v1
- [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md) — phased graph store note
