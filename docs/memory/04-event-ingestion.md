# Event ingestion (episodic `memory_events`)

## Purpose

Workflows and internal agents should record **episodic memory events** through the **append-only** `memory_events` log. This path does **not** write to long-term structured memory (items, semantic memories, or graph edges). No inference or automatic semantic extraction is performed in this step.

## API

- **POST** `POST /api/v1/memory/events` (requires an unlocked platform session, same as other v1 JSON APIs).
- **201 Created** with body:

```json
{ "id": 123, "occurredAt": "2026-04-26T12:00:00+00:00", "createdAt": "2026-04-26T12:00:00+00:00" }
```

- Request DTO: `IngestMemoryEventV1Request` in `Platform.Contracts` (camelCase in JSON under the host’s `HttpJson` options).

| Field | Description |
| --- | --- |
| `eventType` | Required, non-white space, max 256. |
| `domain` | Optional, max 256. |
| `workflowId` | Optional correlation id when the event arose from a workflow, max 256. |
| `projectId` | Optional correlation id, max 256. |
| `payloadJson` | Optional **JSON** string (valid JSON when present); stored in `jsonb` for structured payloads. |
| `userId` | Optional; `0` or `null` resolves to the default `memory_users` row (id `1`). In the current single-tenant deployment, only `0`/`1` is accepted. |
| `occurredAt` | Optional; if omitted, server time is used. Must not be more than ~1 minute in the future (clock skew guard). |

## Application flow

1. **Command:** `IngestMemoryEventCommand` in `Application/Features/Memory/Events/IngestEvent/`.
2. **Validation:** `IngestMemoryEventCommandValidator` (FluentValidation).
3. **Domain value object:** `UncommittedMemoryEvent.CreateForIngest` enforces invariants (non-empty `eventType` after trim, time skew check).
4. **Persistence:** `EfMemoryEventWriter` implements `IMemoryEventWriter` by inserting a new `MemoryEvent` row only. **No updates or deletes** on `memory_events` are performed by the writer; this keeps the table **append-only** at the application level.

`CreatedAt` on the row is set at insert time; `occurredAt` is either supplied by the client or defaults to the same ingest moment (both UTC).

## No semantic side effects

Event ingestion does **not** run semantic memory extraction, review-queue promotion, or relationship inference. Those are separate pipelines that may be triggered later from events if needed.

## Related

- [02-db-schema.md](02-db-schema.md) — `memory_events` table
- [03-domain-model.md](03-domain-model.md) — `MemoryEvent`, `UncommittedMemoryEvent`
