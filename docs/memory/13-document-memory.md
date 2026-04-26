# Document memory v1

## Purpose

**Document memory** stores **long-form artifacts** (architecture write-ups, session summaries, project notes, research saves, generated reports) as **`memory_items`** with `MemoryType = Document`. They are **evidence and context** for agents and workflows—not automatic **semantic truth**.

- **Retrieval:** chunked **pgvector** embeddings (when a generator is configured) plus **`GetMemoryContext`** (`memoryItemVectorRecalls`, optional project/domain scoping).
- **Truth claims:** anything that should be treated as durable “what the user believes is true” still belongs in **`semantic_memories`** and, when created from inference, the **review queue** ([08-semantic-memory.md](08-semantic-memory.md), [07-review-queue.md](07-review-queue.md)).

---

## Data model

### `memory_items` (documents)

| Field | Role |
| --- | --- |
| `MemoryType` | **`Document`** for this feature. |
| `Title` | Short label (search + chunk canonical prefix). |
| `Content` | Full body (`text`). |
| `SourceType` | Origin label (e.g. `architecture-doc`, `session-export`). |
| `ProjectId` | Optional scope (string, ≤256 chars); used for **vector recall filtering** when the context request includes `projectId`. |
| `Domain` | Optional topical scope (string, ≤256 chars); same for **`domain`** on the context request. |
| `AuthorityWeight` | Ingest uses a **document-evidence** band (~**0.62**), below explicit profile truth and below user-approved semantics. |

Documents are **active** after ingest (v1 API promotes on write).

### `memory_embeddings` (chunked vectors)

| Field | Role |
| --- | --- |
| `ChunkIndex` | **0..N-1** for documents split into N chunks; **0** for non-document items. |
| `EmbeddedText` | Chunk body (or whole-item canonical text) stored for **recall previews** in `GetMemoryContext`. |
| `ContentSha256` | Hash of the **canonical string embedded** for that row (idempotency on re-upsert). |

**Uniqueness:** `(UserId, MemoryItemId, EmbeddingModelKey, ChunkIndex)`.

**Chunking (defaults, overridable via `DocumentMemory` config):**

- If document `Content` length ≤ **`MaxCharsBeforeChunking`** (default **1800**): **one** vector using the same canonical form as other memory items (`title` + U+001F + full `content`).
- Otherwise: sliding windows of **`MaxChunkBodyChars`** (default **1800**) with **`OverlapChars`** (default **200**). Canonical per chunk: `title` + U+001F + `chunk:{index}` + U+001F + chunk body (see `MemoryEmbeddingCanonicalText.ForDocumentChunk`).

Re-running **`POST /api/v1/memory/embeddings/upsert`** for an item **replaces** all embedding rows for that `(user, item, model key)` set (delete + insert), so chunk counts stay in sync with the current body.

---

## HTTP (v1)

Requires an **unlocked** platform session (same as other memory v1 routes).

| Method | Path | Body | Result |
| --- | --- | --- | --- |
| `POST` | `/api/v1/memory/documents` | `IngestDocumentMemoryV1Request` | `IngestDocumentMemoryV1Response` |

**`IngestDocumentMemoryV1Request` (highlights)**

- `title`, `content` (required).
- `sourceType`, `projectId`, `domain` optional.
- `indexEmbeddings` (default **true**): after save, runs the governed embedding upsert. If no generator is configured, the item is still stored; `embeddingChunksWritten` may be **0**.

**`POST /api/v1/memory/embeddings/upsert`**

Response includes **`chunksWritten`** (always ≥ **1** on success).

---

## `GetMemoryContext` behavior

- Vector hits include **`chunkIndex`**, **`isDocumentEvidence`** (true when `memoryType` is `Document`), **`projectId`**, **`domain`**, **`sourceType`** when present on the item.
- When the context request includes **`projectId`** and/or **`domain`**, **document-typed** hits are **restricted** to rows whose metadata is **unset** (global doc) or **matches** the request. Non-document memory items are **not** filtered this way (only `MemoryType == Document` is gated).

---

## Product rules

1. **Documents ≠ semantics:** do not treat document text as approved claims; route durable facts through **semantic memory** and **review** when needed.
2. **Governed vectors:** only **`IMemoryEmbeddingUpsertService`** (and flows that call it, such as document ingest) write `memory_embeddings`; search always joins back to **`memory_items`**.
3. **pgvector:** same requirements as [12-vector-memory.md](12-vector-memory.md) (`pgvector/pgvector` image or equivalent).

---

## Configuration

`appsettings` section **`DocumentMemory`** binds **`DocumentMemoryChunkingOptions`**:

| Property | Default | Role |
| --- | ---: | --- |
| `MaxChunkBodyChars` | 1800 | Max characters per chunk body. |
| `OverlapChars` | 200 | Overlap between consecutive chunks. |
| `MaxCharsBeforeChunking` | 1800 | Below this, embed as a single vector (whole-item canonical). |
| `MaxEmbeddedTextStoredChars` | 32000 | Cap for `EmbeddedText` column per row. |

---

## See also

- [02-db-schema.md](02-db-schema.md) — column reference
- [03-domain-model.md](03-domain-model.md) — `MemoryItem`, `MemoryEmbedding`
- [06-retrieval-engine.md](06-retrieval-engine.md) — context assembly
- [12-vector-memory.md](12-vector-memory.md) — pgvector and recall
