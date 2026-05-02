# Side Learning — Full Implementation Plan

## Vision

A fully AI-driven learning experience where the platform proposes topics based on your memory, generates a personalized session, collects your feedback throughout, and continuously learns from your engagement to improve future sessions.

---

## Current State

| Layer | What Exists |
|---|---|
| **Frontend** | Mock-only UI. Hardcoded topics, no real API calls, no multi-step flow. |
| **Backend (.NET)** | `SideLearningTopic` entity with a static read repository. `GET /api/v1/side-learning/topics` returns DB rows. No session lifecycle, no memory integration. |
| **Workers (Python)** | Skeleton `SideLearningWorkflow` with one placeholder activity that returns a hardcoded string. |
| **Memory** | Fully designed and implemented on .NET side. Workers have only a stub `MemoryClient` protocol with no HTTP implementation. |

---

## Target Flow

### User-Facing Flow

```
1. User opens side learning (optional: types a prompt or interest hint)
2. Platform proposes 3–5 topic candidates  ← worker generates these from memory context
3. User picks one or gives feedback ("more advanced", "something else about X")
4. Worker generates a full session draft
5. Platform shows the session + adds any memory proposals to the review queue (inbox)
6. User does the session, marks sections complete
7. At the end: user writes a short reflection (what landed, what didn't, any questions)
8. Worker analyzes the reflection and adds more memory proposals to the review queue
```

### Worker-Internal Flow (per stage)

```
Stage A — Topic Proposal:
  1. Fetch MemoryContext (profile, semantic memories, procedural rules, past session docs)
  2. Generate 3–5 candidate topics using LLM
  3. Filter out topics covered recently (check past session MemoryItems)
  4. Return candidates to .NET via internal API

Stage B — Session Generation:
  1. Receive chosen topic + optional user feedback text
  2. Fetch MemoryContext again (now with topic as taskDescription)
  3. Generate full session content (sections: mental model, build it, reflect)
  4. Analyze what memory conclusions can be drawn from the user's choice + feedback
  5. Return session content + memory proposals to .NET via internal API

Stage C — Reflection Analysis:
  1. Receive completed session + user's reflection text
  2. Analyze reflection for learning style signals, depth preferences, topic interest level
  3. Generate memory proposals (semantic memory candidates + possible procedural rule updates)
  4. Post proposals to review queue via internal API
```

---

## Architecture

The key design principle across all of this: **workers never own state**. They compute and call back to .NET. The .NET API is the single source of truth for sessions, proposals, and memory.

Each stage is a **separate Temporal workflow run** tied to a `WorkflowRun` row. This avoids long-running workflows that span user interaction time (which could be hours between topic selection and completing the session).

```
Frontend ←→ .NET API ←→ SideLearningSession (DB)
                 ↑↓
             WorkflowRun (Temporal)
                 ↑↓
         workers-platform (Python)
                 ↓
         .NET Internal API (memory writes, session updates)
```

---

## Phase 1 — Backend Session Model

### New Domain Entity: `SideLearningSession`

This tracks the full lifecycle of one session end-to-end.

```
Phases (enum SideLearningSessionPhase):
  ProposingTopics        — worker is generating topic candidates
  AwaitingTopicSelection — proposals returned, waiting for user to pick
  GeneratingSession      — worker is generating session content
  SessionReady           — draft is ready, user can start
  InProgress             — user is actively working through sections
  AwaitingReflection     — session completed, waiting for reflection input
  AnalyzingReflection    — worker is analyzing reflection
  Completed              — full cycle done, memory proposals queued
  Failed                 — something went wrong
```

Key fields: `Id`, `UserId`, `Phase`, `InitialPrompt` (nullable), `SelectedTopicTitle`, `SelectedTopicReason` (user's feedback text), `SessionContentJson` (the generated sections), `ReflectionText`, `WorkflowRunId` (current active run), `CreatedAt`, `UpdatedAt`.

### New DB Table: `side_learning_sessions`

EF migration after domain entity is defined. The session content is stored as `jsonb` (same reasoning as profile — always read as a whole, flexible schema for sections).

### New .NET API Endpoints

```
POST /api/v1/side-learning/sessions
  Body: { initialPrompt?: string }
  Action: Creates session in ProposingTopics phase, starts Temporal WorkflowRun (Stage A)
  Returns: { sessionId, phase, workflowRunId }

GET  /api/v1/side-learning/sessions/{id}
  Returns: current session state (phase + relevant data for that phase)

POST /api/v1/side-learning/sessions/{id}/select-topic
  Body: { topicTitle: string, feedback?: string }
  Action: Sets SelectedTopicTitle + feedback, transitions to GeneratingSession,
          starts new WorkflowRun (Stage B)

POST /api/v1/side-learning/sessions/{id}/progress
  Body: { sectionId: string, completed: boolean }
  Action: Tracks section completion. Transitions to AwaitingReflection when all done.

POST /api/v1/side-learning/sessions/{id}/reflect
  Body: { reflection: string }
  Action: Saves reflection text, transitions to AnalyzingReflection,
          starts new WorkflowRun (Stage C)

GET  /api/v1/side-learning/sessions
  Returns: list of sessions (history view)
```

### New Internal API Endpoints (for workers)

These use the internal API key pattern (`X-Internal-Key` header), same as the nightly consolidation route.

```
POST /api/internal/v1/side-learning/sessions/{id}/proposals
  Body: { topics: [{ title, rationale, estimatedMinutes, difficulty }] }
  Action: Saves topic proposals onto the session, transitions to AwaitingTopicSelection

POST /api/internal/v1/side-learning/sessions/{id}/session-content
  Body: { sections: [...], memoryProposals: [...] }
  Action: Saves generated session content, transitions to SessionReady.
          memoryProposals are passed through to CreateReviewQueueItemCommandHandler.

POST /api/internal/v1/side-learning/sessions/{id}/reflection-insights
  Body: { memoryProposals: [...] }
  Action: Posts proposals to review queue, transitions to Completed.
          Also ingests a MemoryEvent and a Document MemoryItem for the session.
```

### Session Content Schema (jsonb)

```json
{
  "sections": [
    {
      "id": "mental-model",
      "label": "Mental Model",
      "content": "...",
      "type": "concept"
    },
    {
      "id": "build-it",
      "label": "Build It",
      "content": "...",
      "type": "hands-on"
    },
    {
      "id": "reflect",
      "label": "Reflect",
      "content": "...",
      "type": "reflection-prompt"
    }
  ]
}
```

---

## Phase 2 — Worker: Topic Proposal (Stage A)

### New Python Files

```
workers-platform/app/workflows/side_learning/
  workflow.py          ← refactor to dispatch by stage
  activities.py        ← all activities
  contracts.py         ← typed input/output models for this workflow
  memory_mapper.py     ← helpers for building memory proposals from LLM output

workers-platform/app/memory/client/
  adapter.py           ← existing Protocol (keep)
  http_client.py       ← NEW: real HTTP implementation
  models.py            ← NEW: typed models for MemoryContext response
```

### `SideLearningWorkflowInput` Contract

```python
class SideLearningStage(str, Enum):
    PROPOSE_TOPICS = "propose_topics"
    GENERATE_SESSION = "generate_session"
    ANALYZE_REFLECTION = "analyze_reflection"

class SideLearningWorkflowInput(BaseModel):
    stage: SideLearningStage
    session_id: str
    workflow_run_id: str
    # stage-specific fields:
    initial_prompt: str | None = None        # Stage A
    topic_title: str | None = None           # Stage B
    user_feedback: str | None = None         # Stage B
    reflection_text: str | None = None       # Stage C
    session_content_json: str | None = None  # Stage C (pass the session for context)
```

### Stage A Activities

**`fetch_memory_context_for_learning`**
- Calls `POST /api/v1/memory/context` with `workflowType: "side_learning"`, `domain: "learning"`, and `taskDescription` from `initialPrompt` if provided
- Returns typed `MemoryContextV1` model

**`propose_learning_topics`**
- Takes memory context as input
- Builds a prompt including: profile interests + skill levels, semantic memories (domain: learning), procedural rules (workflowType: side_learning), recent session document titles from vector recalls
- Calls LLM, asks for 5 topic candidates as structured JSON: `{ title, rationale, estimatedMinutes, difficulty, targetSkillGap }`
- Returns list of proposals

**`filter_known_topics`**
- Takes proposals + memory context
- Filters out any topic whose title closely matches a recent session document (from `memoryItemVectorRecalls`)
- Returns filtered list (minimum 3)

**`post_topic_proposals`**
- Calls `POST /api/internal/v1/side-learning/sessions/{id}/proposals`
- Returns success/failure

### Stage A Workflow

```python
@workflow.run
async def run(self, input: SideLearningWorkflowInput) -> WorkflowRunResult:
    if input.stage == SideLearningStage.PROPOSE_TOPICS:
        context = await workflow.execute_activity("fetch_memory_context_for_learning", ...)
        proposals = await workflow.execute_activity("propose_learning_topics", context, ...)
        filtered = await workflow.execute_activity("filter_known_topics", proposals, context, ...)
        await workflow.execute_activity("post_topic_proposals", input.session_id, filtered, ...)
        return WorkflowRunResult(status="completed", ...)
```

---

## Phase 3 — Worker: Session Generation (Stage B)

### Stage B Activities

**`generate_learning_session`**
- Fetches memory context again with topic title as `taskDescription`
- Builds a prompt including: user's chosen topic, their optional feedback ("make it more advanced"), profile skill levels, relevant past sessions (vector recalls), semantic memories about learning style, procedural rules
- Generates a session with 3+ sections: Mental Model, Build It, Reflect prompt
- Returns structured `SessionContent`

**`analyze_topic_selection_for_memory`**
- Input: chosen topic, user feedback text, memory context
- Asks LLM: "Given this user chose this topic with this feedback, what can we conclude that is worth saving to memory?"
- Only proposes things with sufficient evidence or clear signal from the feedback text
- Returns list of `MemoryProposal` objects: `{ proposalType, title, summary, proposedChangeJson, evidenceJson, priority }`

**`post_session_content`**
- Calls `POST /api/internal/v1/side-learning/sessions/{id}/session-content`
- Sends both session sections and memory proposals

---

## Phase 4 — Worker: Reflection Analysis (Stage C)

### Stage C Activities

**`analyze_reflection`**
- Input: reflection text + original session content + memory context
- Asks LLM to analyze: what did the user find valuable? what was too shallow or too deep? any new interests revealed? any procedural signals (e.g. "I learn better with more code examples")?
- Returns list of `MemoryProposal` objects

**`post_reflection_insights`**
- Calls `POST /api/internal/v1/side-learning/sessions/{id}/reflection-insights`

**`ingest_session_as_document`**
- Separately (also from Stage C or from the internal endpoint): ingest the completed session + reflection as a Document `MemoryItem` via `POST /api/v1/memory/documents`
- This enables vector search on past sessions for future topic filtering
- Title: `"Side Learning: {topicTitle} — {date}"`
- Content: session sections + reflection, concatenated
- Domain: `"learning"`

**`ingest_session_event`**
- Ingest a `MemoryEvent` record of the completed session
- EventType: `"side_learning.session_completed"`
- Domain: `"learning"`
- PayloadJson: `{ topicTitle, durationMinutes, sectionsCompleted, reflectionLength }`

---

## Phase 5 — Frontend

### What Changes

The frontend replaces its mock-data flow with the real multi-step API flow. The existing UI shell (topic list, session view) is a reasonable starting point but needs to be wired to session state from the backend.

### New UI States

```
1. Initiate
   - "Start a learning session" button
   - Optional: text input for a topic hint or interest ("I want to learn something about Temporal")
   - Calls POST /api/v1/side-learning/sessions
   - Transitions to: Loading (polling session phase)

2. Topic Selection  (phase: AwaitingTopicSelection)
   - Shows 3–5 AI-generated topic cards with title, rationale, estimated time, difficulty
   - Each card has a "Choose" button
   - Optional text field: "Not quite what I wanted — give feedback" → re-triggers Stage A with feedback
   - Calls POST /api/v1/side-learning/sessions/{id}/select-topic

3. Session Loading  (phase: GeneratingSession)
   - Spinner / skeleton while worker generates
   - Polls GET /api/v1/side-learning/sessions/{id} until phase === SessionReady

4. Session View  (phase: SessionReady / InProgress)
   - Existing section-by-section layout (already built with mocks)
   - Each "Mark done" calls POST /api/v1/side-learning/sessions/{id}/progress
   - When all sections done → shows "Write your reflection"

5. Reflection  (phase: AwaitingReflection)
   - Text area: "What landed? What didn't? Any questions or new interests?"
   - Submit button calls POST /api/v1/side-learning/sessions/{id}/reflect

6. Analyzing  (phase: AnalyzingReflection)
   - Brief loading state with message like "Analyzing your session..."
   - Polls until phase === Completed

7. Done  (phase: Completed)
   - Summary card: topic, time taken, sections completed
   - Small notice: "Memory proposals added to your inbox" (links to review queue)
   - "Start another session" button
```

### Memory Inbox Notice

After Stage B (session ready) and Stage C (reflection analyzed), there may be proposals in the review queue. The UI should show a subtle indicator — not a blocker — pointing to the memory review queue. Something like a badge on the nav or a soft banner: "Jarvis noticed a few things worth saving — review them in your memory inbox."

---

## Memory Integration Summary

| Moment | What Gets Written | Where |
|---|---|---|
| User selects topic (Stage B start) | Nothing yet — just signals intent | — |
| Worker posts session content (Stage B end) | 0–3 review queue proposals from topic selection analysis | `memory_review_queue` |
| User completes all sections | Nothing — pure UI state | — |
| User submits reflection (Stage C end) | 0–5 review queue proposals from reflection analysis | `memory_review_queue` |
| Stage C wraps up | One `MemoryEvent` (session completed) | `memory_events` |
| Stage C wraps up | One `Document` MemoryItem (full session + reflection) | `memory_items` + `memory_embeddings` |
| Nightly consolidation (next day) | Semantic memory updates derived from session events over time | `semantic_memories` (via review queue) |

**What never happens automatically:** no SemanticMemory or ProceduralRule rows are written directly by the worker. The worker only proposes — the user approves via the review queue.

---

## Implementation Order

### Iteration 1 — Backend skeleton + internal auth
- [ ] `SideLearningSession` domain entity + enum
- [ ] EF migration: `side_learning_sessions` table
- [ ] `SideLearningSessionCommandHandler` for each phase transition
- [ ] All user-facing API endpoints (POST sessions, GET session, select-topic, progress, reflect)
- [ ] All internal API endpoints (proposals, session-content, reflection-insights)
- [ ] Internal API key middleware wired to side learning internal routes

### Iteration 2 — Memory HTTP client in workers
- [ ] `HttpMemoryClient` implementing `MemoryClient` protocol
- [ ] Typed `MemoryContextV1` Pydantic model (mirrors .NET `MemoryContextV1Dto`)
- [ ] Config: `PLATFORM_API_BASE_URL` + `PLATFORM_INTERNAL_API_KEY` in settings
- [ ] Unit tests for HTTP client

### Iteration 3 — Worker Stage A (topic proposals)
- [ ] `fetch_memory_context_for_learning` activity
- [ ] `propose_learning_topics` activity (LLM call with structured output)
- [ ] `filter_known_topics` activity
- [ ] `post_topic_proposals` activity
- [ ] Workflow dispatch for `propose_topics` stage
- [ ] Integration test: full Stage A against real .NET API

### Iteration 4 — Worker Stage B (session generation)
- [ ] `generate_learning_session` activity (LLM call, structured sections)
- [ ] `analyze_topic_selection_for_memory` activity
- [ ] `post_session_content` activity
- [ ] Workflow dispatch for `generate_session` stage

### Iteration 5 — Worker Stage C (reflection)
- [ ] `analyze_reflection` activity
- [ ] `post_reflection_insights` activity
- [ ] `ingest_session_as_document` activity
- [ ] `ingest_session_event` activity
- [ ] Workflow dispatch for `analyze_reflection` stage

### Iteration 6 — Frontend
- [ ] Remove all mock data
- [ ] `useSideLearningSession` hook: create session, poll phase, trigger transitions
- [ ] Initiate screen
- [ ] Topic selection screen
- [ ] Session loading / skeleton
- [ ] Session view wired to real content (sections from API)
- [ ] Reflection screen
- [ ] Done screen with memory inbox notice
- [ ] History list wired to `GET /api/v1/side-learning/sessions`

### Iteration 7 — Polish + procedural rules
- [ ] Seed 2–3 starter procedural rules for `side_learning` workflow type (e.g. "always include a hands-on section", "structure sessions as: mental model → build → reflect")
- [ ] Validate these rules come back in MemoryContext and the LLM prompt actually uses them
- [ ] Add session progress tracking (what percent of sessions result in proposals, acceptance rate)

---

## Open Questions

1. **LLM model choice for workers** — which model for topic generation vs session generation? Session generation is longer-form, topic generation can be faster/cheaper.
2. **Section count and structure** — 3 sections (mental model, build, reflect) is the default. Should the worker be able to vary this based on topic complexity or user's available time from their profile?
3. **Re-propose flow** — when the user gives feedback on topic proposals and wants new ones, does that start a new WorkflowRun entirely, or does the same workflow re-run Stage A internally?
4. **Polling vs WebSocket** — frontend polls `GET /api/v1/side-learning/sessions/{id}` while workers are running. Acceptable for now. Worth revisiting if generation latency is high.
5. **Minimum reflection length** — enforce a minimum before allowing submission, or leave it open? A very short reflection gives the analysis worker nothing useful.
