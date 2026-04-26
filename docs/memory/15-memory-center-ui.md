# Memory Center UI (frontend v1)

The **Memory Center** is a first-class area in the web shell (`frontend-platform`) for **inspecting and steering** long-term memory: what you said explicitly, what the system inferred, what’s waiting for review, recent activity, and procedural behavior rules.

It follows the same **visual language** as the rest of the platform: `AppShell` + sidebar, **Orbitron / Space Mono** accents, **Jarvis** components (`JarvisCard`, `JarvisButton`, `JarvisTag`, `JarvisInlineError`), and CSS module tokens from `globals.css` (`--accent`, `--color-text-muted`, etc.).

---

## Location

- **Base path:** `/memory` (redirects to `/memory/profile`).
- **Nav:** “Memory” in the primary sidebar ([`nav-items.ts`](../../frontend-platform/src/components/layout/nav-items.ts)), with icon **`brain`**.
- **Route titles:** Top bar uses [`getRouteMeta`](../../frontend-platform/src/lib/shell/route-meta.ts) — prefix `/memory` → *Memory / center*.

---

## Sections (pages)

| Path | Role |
| --- | --- |
| `/memory/profile` | **Profile memory** — edit explicit goals and interests (`PUT /api/v1/memory/explicit-profile`). Copy explains that user-entered data is high-trust. |
| `/memory/learned` | **Learned about me** — list semantic memories with **How sure we are** and **Source reliability** bars, status pill, link to **Why this is here** (`GET /api/v1/memory/semantics`). |
| `/memory/learned/[id]` | **Memory detail** — full claim, plain-language *why* text, the same two bars, and **supporting activity** from `GET /api/v1/memory/semantics/{id}/evidence` when available. |
| `/memory/review` | **Review queue** — pending items only; **Add to memory** / **Not now** call approve/reject (`POST` … `/approve`, `/reject`). Hides low-level JSON; suggests details stay in the system. |
| `/memory/timeline` | **Activity** — reverse-chronological episodic list (`GET /api/v1/memory/events`). |
| `/memory/procedural` | **Procedural rules** — list summaries with status, version, priority, source, and a **Reliability** bar (`GET /api/v1/memory/procedural-rules`). |

Sub-navigation is a small pill row (`MemorySubNav`) so users can move between sections without leaving Memory Center.

---

## Copy and UX rules

- **Confidence** is labeled **“How sure we are”** (0–100% bar). Short hint: patterns over time, not a test score.
- **Authority** is labeled **“Source reliability”** (or **“Reliability”** on rules). Hint: your direct input and approvals increase it.
- **Statuses** are humanized: e.g. `PendingReview` → *Suggested* / *suggested for you to confirm*; we avoid enum names in headings.
- **We do not** show internal codes (`v1-sql+vector`, table names, worker names) in this UI.
- **Evidence** for a learned memory: when links exist, we show event type, time, optional note, and a simple relevance %; if none, we use reassuring copy instead of empty technical jargon.

---

## API surface (v1)

The UI calls the public memory HTTP API under `/api/v1/…` (session cookie, same as other app modules), via [`memory-center.ts`](../../frontend-platform/src/lib/api/adapters/memory-center.ts):

| Action | Method / path (summary) |
| --- | --- |
| Load / save explicit profile | `GET` / `PUT` `memory/explicit-profile?userId=0` |
| List semantics | `GET` `memory/semantics?userId=0&includePending=true` |
| Semantic detail | `GET` `memory/semantics/{id}?userId=0` |
| Evidence rows | `GET` `memory/semantics/{id}/evidence?userId=0` |
| Review list / actions | `GET` `memory/review-queue`, `POST` …/approve, …/reject |
| Timeline | `GET` `memory/events?userId=0&take=…` |
| Procedural list | `GET` `memory/procedural-rules?userId=0` |

`userId=0` lets the backend resolve the default user in the current single-tenant setup.

**Backend additions for this UI:** `GET /api/v1/memory/events` (recent episodic events) and `GET /api/v1/memory/semantics/{id}/evidence` (evidence + joined event for display). Both are read-only, governed, and user-scoped.

---

## File map (frontend)

- [`src/app/memory/`](../../frontend-platform/src/app/memory/) — `layout`, redirect `page`, and section routes.
- [`src/modules/memory-center/`](../../frontend-platform/src/modules/memory-center/) — panels, `MemorySubNav`, `ScoreBar`, `memory-center.module.css`.
- [`src/lib/api/adapters/memory-center.ts`](../../frontend-platform/src/lib/api/adapters/memory-center.ts) — API adapter.

---

## How this ties to the master spec

Memory Center is the user-facing “inspectable and editable” surface for [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md): explicit profile (highest authority), learned semantics, review before important inferred truth, episodic traceability, and procedural rules—without exposing internal pipeline names.

---

## See also

- [05-profile-memory.md](05-profile-memory.md) — explicit profile model
- [08-semantic-memory.md](08-semantic-memory.md) — learned claims
- [07-review-queue.md](07-review-queue.md) — review flow
- [11-procedural-memory.md](11-procedural-memory.md) — procedural rules
- [04-event-ingestion.md](04-event-ingestion.md) — episodic events
- [06-retrieval-engine.md](06-retrieval-engine.md) — `GetMemoryContext` (not shown in this UI; data comes from the same store)
