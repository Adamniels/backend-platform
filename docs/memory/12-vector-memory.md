# Vector memory (pgvector) v1

## Purpose

**Vector retrieval** augments `GetMemoryContext` with **similarity-ranked** rows from governed **`memory_items`**, using PostgreSQL **pgvector**. Vectors are **retrieval support only**: canonical text and lifecycle remain on `memory_items` (including **document**-typed items, which are `memory_items` with `MemoryType = Document`).

PostgreSQL is the **source of truth** for embeddings once written; there is **no separate “vector memory”** store outside SQL.

---

## Requirements

- PostgreSQL with the **`vector`** extension available (shared library + `CREATE EXTENSION vector`).
- Local dev: `backend-platform/docker-compose.yml` uses **`pgvector/pgvector:pg17`**. If you previously ran plain `postgres:17` with the same volume, **recreate the volume** (or run `CREATE EXTENSION vector` manually on an image that supports it).

---

## Schema

Table **`memory_embeddings`** (migration **`MemoryEmbeddingsV1`**). See [02-db-schema.md](02-db-schema.md).

| Column | Role |
| --- | --- |
| `UserId` | Tenancy (FK → `memory_users`). |
| `MemoryItemId` | **Required** FK → `memory_items` (cascade delete). |
| `EmbeddingModelKey` | Logical model id (e.g. `deterministic-recall-v1`, `text-embedding-3-small`). |
| `EmbeddingModelVersion` | Optional provider version string. |
| `Dimensions` | Must match `vector(N)` width (v1: **1536**). |
| `ContentSha256` | SHA-256 hex of canonical embedded text for that row (whole item or per-chunk canonical; see [13-document-memory.md](13-document-memory.md)). |
| `ChunkIndex` | Integer chunk index; long documents may have many rows per item + model key. |
| `EmbeddedText` | Stored slice for **preview** in `GetMemoryContext` vector hits. |
| `Embedding` | `vector(1536)` column. |
| `CreatedAt` / `UpdatedAt` | Audit. |

**Indexes**

- **Unique** `(UserId, MemoryItemId, EmbeddingModelKey, ChunkIndex)` — one row per chunk per model key (migration **`DocumentMemoryV1`**).
- **HNSW** `ix_memory_embeddings_embedding_hnsw` on `Embedding` with `vector_cosine_ops` for approximate cosine search.

---

## Application ports

| Port | Location | Role |
| --- | --- | --- |
| `IMemoryEmbeddingGenerator` | `Platform.Application/Abstractions/Memory/Embeddings/` | Produces a query vector from task text (or returns null if disabled). |
| `IMemoryVectorRecallSearch` | same | pgvector similarity search joined to **`memory_items`** (Active only). |
| `IMemoryEmbeddingUpsertService` | same | **Governed** upsert: loads the memory item, verifies ownership, hashes content, writes/updates `memory_embeddings`. |

Implementations live under `Platform.Infrastructure/Features/Memory/Embeddings/`.

**Defaults**

- **`Testing`** environment or `MemoryVector:UseDeterministicEmbeddingGenerator=true` → **`DeterministicRecallEmbeddingGenerator`** (no external API; stable L2-normalized pseudo-embeddings).
- Otherwise → **`NoOpMemoryEmbeddingGenerator`** (vector recall skipped with a warning unless a real provider is wired later).

---

## HTTP (v1)

Requires an **unlocked** platform session.

| Method | Path | Body | Result |
| --- | --- | --- | --- |
| `POST` | `/api/v1/memory/embeddings/upsert` | `UpsertMemoryEmbeddingV1Request` | `UpsertMemoryEmbeddingV1Response` (includes **`chunksWritten`**) |
| `POST` | `/api/v1/memory/documents` | `IngestDocumentMemoryV1Request` | `IngestDocumentMemoryV1Response` (see [13-document-memory.md](13-document-memory.md)) |

`POST /api/v1/memory/context` accepts optional **`includeVectorRecall`** (default **true**). Response adds:

- `memoryItemVectorRecalls` — hits with `memoryItemId`, **`chunkIndex`**, `memoryType`, previews, `cosineSimilarity`, `authorityWeight`, `rankScore`, `embeddingModelKey`, **`isDocumentEvidence`**, optional **`projectId`**, **`domain`**, **`sourceType`**.
- `vectorRecallUsed` — whether any hit was returned.
- `assemblyStage` — `v1-sql+vector` when recall ran with a configured generator; otherwise `v1-sql`.

Warnings may include `vector_recall_disabled` or `vector_recall_unavailable`.

---

## Rules (product)

1. **No ungoverned writes:** only `IMemoryEmbeddingUpsertService` (or future approved workers) may insert/update `memory_embeddings`; callers must target an existing **`memory_items`** row.
2. **Governed mapping:** search SQL always **joins** `memory_embeddings` → `memory_items`; results never float without a memory item id.
3. **Documents** are represented as **`memory_items`** with `MemoryType = Document` (no separate `documents` table in v1).

---

## See also

- [02-db-schema.md](02-db-schema.md) — column reference
- [13-document-memory.md](13-document-memory.md) — long-form documents, chunking, ingest API
- [06-retrieval-engine.md](06-retrieval-engine.md) — context assembly
- [MEMORY_MODULE_README.md](MEMORY_MODULE_README.md) — compose / infra notes
- [03-domain-model.md](03-domain-model.md) — ports overview
