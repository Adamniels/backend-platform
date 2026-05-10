# News Phase 1 — operations (schedule + manual trigger)

## Prerequisites

- `Platform.Api` with `Temporal:Address` pointing at your Temporal cluster when you want real workflow execution (empty uses `StubWorkflowStarter` and runs appear as **failed** in `WorkflowRuns`).
- `PlatformWorkers:ServiceToken` set on the API; workers use the same value as `PLATFORM_INTERNAL_SERVICE_TOKEN` (legacy: `MEMORY_WORKER_SERVICE_TOKEN`).
- `workers-platform` running with `TEMPORAL_TASK_QUEUE=platform` (default) so `NewsIntelligenceWorkflow` is polled.
- Optional: `GNEWS_API_KEY` on workers for GNews; without it, GNews returns no articles and other sources still run.

## Nightly schedule (Temporal)

Phase 1 uses a **once-per-night** cadence (not six-hourly). Create or update a **Temporal Schedule** in your namespace, for example **05:00 UTC** daily with overlap skipped:

```bash
temporal schedule create \
  --schedule-id "news-intelligence-nightly-utc" \
  --workflow-type "NewsIntelligenceWorkflow" \
  --task-queue "platform" \
  --cron "0 5 * * *" \
  --overlap-policy Skip
```

- **`Skip`**: if a run is still in progress when the next tick fires, the new run is skipped (no pile-up).
- Adjust cron for your ops window; keep **UTC** unless you standardize on another zone across services.
- Pause/disable: pause or delete the schedule in Temporal UI/CLI (no app-level flag required).

The schedule payload must match what `Platform.Api` sends when starting workflows: JSON with `name`, `workflowType`, `taskQueue`, `workflowRunId` (the .NET `StartWorkflowRun` path supplies this via `TemporalWorkflowStarter`).

## Manual trigger (internal API)

On-demand ingest uses the same workflow as the schedule, via **Bearer-authenticated** internal routes:

```http
POST /api/internal/v1/news/intelligence/runs
Authorization: Bearer <PlatformWorkers:ServiceToken>
Content-Type: application/json

{ "name": "News intelligence (adhoc)" }
```

`name` is optional; default display name is `News intelligence (manual)`.

**Response:** `WorkflowRunSummaryDto` (`id`, `name`, `status`, `updatedAt`) — same shape as `POST /api/v1/workflow-runs`. If Temporal is not configured, `status` will be `failed` but the HTTP call still returns 200 with that body.

**Ingest endpoint** (used by the worker activity, not typically by humans):

```http
POST /api/internal/v1/news/items
Authorization: Bearer <token>
```

Body: `IngestNewsItemV1Request` (`title`, `url`, `source`, `body`, `author?`, `publishedAt` ISO string, `sourceFeedUrl?`). Response: `{ "status": "created"|"duplicate", "id": "..." }`.

## Worker configuration (optional JSON lists)

| Variable | Purpose |
|----------|---------|
| `NEWS_RSS_FEED_URLS_JSON` | JSON array of RSS URLs; empty → built-in defaults |
| `NEWS_GNEWS_TOPICS_JSON` | JSON array of topic strings; empty → defaults |
| `NEWS_ARXIV_CATEGORIES_JSON` | JSON array of arXiv category codes; empty → defaults |
| `GNEWS_API_KEY` | GNews API token; empty → GNews skipped |

See `workers-platform/app/runtime/config/settings.py` for field definitions.
