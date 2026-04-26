# Explicit profile memory (v1)

## Purpose

**User-entered** profile content is the **highest-authority** source for how the platform should treat the person’s goals, interests, and working context. It is stored in **dedicated PostgreSQL columns** (typed `text[]` and structured `jsonb`), not as a single opaque text blob, and is **separate** from inferred `memory_items` / `semantic_memories` rows.

Inference pipelines and background jobs **must not** call the explicit-profile write path. They should record hypotheses as `MemoryItem` with `MemoryItemType.Inferred` or as `SemanticMemory` (lower authority by design; see [03-domain-model.md](03-domain-model.md)). That prevents inferred updates from replacing user truth in `memory_explicit_profile`.

## Data model

Table **`memory_explicit_profile`** (see [02-db-schema.md](02-db-schema.md) for column types and indexes). One **unique** row per `UserId` (single-tenant default user id `1`).

| Area | Storage | Notes |
| --- | --- | --- |
| Core interests | `text[]` | Non-empty strings, max count/length enforced in validation + domain. |
| Secondary interests | `text[]` | Same. |
| Goals | `text[]` | Same. |
| Preferences | `jsonb` | Array of `{ "key", "value" }` objects. |
| Active projects | `jsonb` | Array of `{ "name", "externalId"? }` objects. |
| Skill levels | `jsonb` | Array of `{ "name", "level" }` with `level` in **0.0–1.0** (aligns with `AuthorityWeight` / importance scale). |
| Authority | `double precision` | **Always 1.0** for user-driven rows (see `ExplicitUserProfileContent.ExplicitUserAuthorityValue`). |
| Timestamps | `timestamptz` | `CreatedAt` / `UpdatedAt` on insert and each user update. |

## API (v1)

All routes require an **unlocked** platform session (same as other `/api/v1/...` JSON routes).

- **GET** `GET /api/v1/memory/explicit-profile?userId=` (optional; `0` or omitted → default `memory_users` id `1`).
  - If no row exists yet, returns empty collections, `authorityWeight: 1`, and `id: null`.
- **PUT** `PUT /api/v1/memory/explicit-profile?userId=` (optional) with body `UpdateProfileMemoryV1Request` / `ProfileMemoryV1Dto` fields (see `Platform.Contracts`).

Response body shape: `ProfileMemoryV1Dto` (includes `id` after the first save, plus `createdAt` / `updatedAt` when persisted).

## Application layer

- **Query:** `GetProfileMemoryQuery` + `GetProfileMemoryQueryHandler`
- **Command:** `UpdateProfileMemoryCommand` + `UpdateProfileMemoryCommandHandler` + `UpdateProfileMemoryCommandValidator` (FluentValidation, aligned with domain limits)
- **Persistence:** `IExplicitUserProfileRepository` → `EfExplicitUserProfileRepository` (insert or update; **no** delete in v1)
- **Domain:** `ExplicitUserProfile.ApplyUserUpdate` validates and normalises payload, and **always** sets authority to the explicit maximum (1.0)

## Authority and conflict rules

- **Explicit profile row:** `AuthorityWeight` is **fixed at 1.0** for user saves.
- **Inferred content** uses lower fixed weights in value objects (e.g. `AuthorityWeight.Inferred` for semantic material) and must not target this table; there is **no** merge/upsert from inference into `memory_explicit_profile` in v1.

## See also

- [02-db-schema.md](02-db-schema.md) — physical schema
- [03-domain-model.md](03-domain-model.md) — ports and entities
- [MEMORY_SYSTEM_MASTER_ARCHITECTURE.md](MEMORY_SYSTEM_MASTER_ARCHITECTURE.md) — authority model (product-level)
