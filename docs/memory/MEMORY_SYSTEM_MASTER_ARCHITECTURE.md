MEMORY SYSTEM MASTER ARCHITECTURE

Vision

Build a world class long term memory system for a personal AI platform that will be used for 10+ years.

The memory system should become the central intelligence layer of the platform. It should continuously learn, adapt, personalize, preserve context across years, and improve every workflow over time.

This is not chat history storage.
This is not a vector database with random notes.
This is a governed multi layer memory architecture.

Goals:

* remember what matters
* forget what does not matter
* personalize every workflow
* preserve project continuity across years
* learn preferences safely
* remain inspectable and editable by the user
* increase usefulness over time
* become the platform brain

⸻

Core Philosophy

The platform owns memory.
Agents and workflows consume memory through APIs.
Agents do not own scattered private memories.
All meaningful workflows can emit events and evidence.
Memory is consolidated and improved continuously.
The user remains in control of important long term truths.

Principles:

1. Truth over noise
2. Evidence over guessing
3. Explicit user input outranks inference
4. Relevance over volume
5. User trust is mandatory
6. Memory must be inspectable
7. Memory should evolve, decay, merge, and improve

⸻

Memory Layers

1. Explicit Profile Memory

Structured user entered truths.
Highest authority.

Examples:

* core interests
* secondary interests
* goals
* preferred explanation depth
* preferred learning style
* preferred output format
* dislikes
* active projects
* available time budget
* skill levels by topic

Store in PostgreSQL typed relational tables.

⸻

2. Episodic Memory

What happened.
Append only evidence timeline.

Examples:

* completed learning session about JWT
* rated recommendation useful
* skipped beginner frontend article
* asked for deep architecture explanation
* opened Temporal docs repeatedly
* generated architecture report
* accepted memory proposal
* rejected recommendation

Store as event log.

⸻

3. Semantic Memory

Learned understanding derived from repeated evidence.

Examples:

* user prefers systems level explanations
* user values depth over shallow summaries
* backend architecture is stable long term interest
* user learns best through build first sessions
* user currently has growing interest in embedded systems

Requires confidence, evidence links, timestamps.

⸻

4. Procedural Memory

How the platform should behave for the user.

Examples:

* when generating learning sessions include goal, context, hands on, reflection
* when explaining architecture start with mental model then code structure
* avoid repetitive beginner recommendations
* prioritize technical depth over surface level summaries

Versioned behavior rules.

⸻

5. Working Memory

Temporary memory for current workflow execution.

Examples:

* current task state
* current ranking candidates
* current reasoning notes
* current generated draft
* active subgoals

Ephemeral, short lived.

⸻

6. Graph Memory

Connected entities and relationships.

Examples:
Adam -> interested_in -> Backend Architecture
Adam -> works_on -> Personal AI Platform
Personal AI Platform -> uses -> .NET
Personal AI Platform -> uses -> Python Workers
Adam -> learning -> Temporal
Adam -> applied_to -> Embedded Internships

Useful for multi hop reasoning and connected context.

⸻

7. Document Memory

Long form artifacts.

Examples:

* architecture docs
* project notes
* session summaries
* saved research
* strategy writeups
* plans
* reports

Indexed semantically for retrieval.

⸻

Storage Architecture

Use multi store memory architecture.

PostgreSQL, Source of Truth

Stores:

* profile memory
* semantic memories metadata
* episodic events
* procedural rules
* approvals
* review queue
* confidence scores
* memory relationships metadata
* audit logs

Vector Index, pgvector first

Stores embeddings for:

* notes
    n- sessions
* memories
* docs
* conversations
* semantic retrieval

Can migrate later to Qdrant if scale demands.

Graph Database, later phase

Neo4j or similar.
Used for relationship queries and memory graph.

Blob Storage

Files, PDFs, exports, large artifacts.

⸻

Memory Authority Model

Every memory must track authority.

Example levels:

* explicit user entered = 1.00
* user confirmed suggestion = 0.90
* repeated behavioral evidence = 0.70
* strong inference = 0.55
* weak inference = 0.30
* speculative signal = 0.10

Explicit profile memory always outranks inferred memory.

Example:
Core interest entered manually:
backend architecture

Weak inferred interest:
frontend design due to recent project activity

Do not overwrite explicit memory.
Represent both.

⸻

Confidence Model

Every inferred memory tracks:

* confidence
* evidence count
* recency
* consistency over time
* user confirmations
* contradiction count
* authority source

Suggested relevance scoring:

final_relevance =
authority_weight * 0.35 +
semantic_similarity * 0.25 +
recency * 0.15 +
confidence * 0.15 +
workflow_relevance * 0.10

⸻

Canonical Data Model

memory_items

* id
* user_id
* memory_type
* title
* content
* structured_json
* source_type
* authority_weight
* confidence
* importance
* freshness_score
* status
* created_at
* updated_at
* last_accessed_at

memory_events

* id
* user_id
* event_type
* domain
* workflow_id
* project_id
* payload_json
* occurred_at

semantic_memories

* id
* user_id
* key
* claim
* domain
* confidence
* authority_weight
* status
* created_at
* updated_at
* last_supported_at

memory_evidence

* id
* semantic_memory_id
* event_id
* strength
* reason

procedural_rules

* id
* user_id
* workflow_type
* rule_name
* rule_content
* priority
* source
* version
* status

memory_review_queue

* id
* user_id
* proposal_type
* title
* summary
* proposed_change_json
* evidence_json
* priority
* status
* created_at

memory_relationships

* id
* from_entity
* relation_type
* to_entity
* strength
* source
* created_at

⸻

Retrieval Architecture

All workflows call:

GetMemoryContext(task)

Never give direct unrestricted memory access.

The Memory API returns curated context.

Retrieval Pipeline

1. explicit profile facts
2. active goals
3. relevant projects
4. relevant semantic memories
5. relevant episodes
6. procedural rules for this workflow
7. retrieved docs / notes
8. current working state
9. warnings / conflicts

MemoryContext Response

* profileFacts
* activeGoals
* relevantProjects
* semanticMemories
* episodicExamples
* proceduralRules
* retrievedDocuments
* currentState
* conflicts
* warnings

Return relevance ranked context only.
Not everything.

⸻

Nightly Consolidation Engine

Run nightly via Temporal scheduled workflow.
Python workers recommended.

Input windows:

* previous day
* 7 day rolling
* 30 day rolling
* 90 day rolling

Responsibilities:

1. read events
2. detect repeated behavior
3. cluster activity
4. detect trend changes
5. compare against existing semantic memory
6. generate proposals
7. auto update safe confidence changes
8. queue approvals for meaningful updates
9. decay stale memories
10. merge duplicate memories
11. generate new procedural insights

⸻

Auto Apply vs Approval Rules

Auto Apply

Low risk reversible changes.

Examples:

* increase confidence score
* freshness update
* mark recent activity spike
* reinforce confirmed preference

Require Approval

Identity or strategic changes.

Examples:

* new core interest
* downgrade core interest
* change learning style defaults
* new long term goal
* persistent topic trend over weeks

Never Auto Save

* sensitive conclusions
* emotional assumptions
* speculative identity claims
* private personal interpretations

⸻

Conflict Handling

Example:
Explicit memory:
backend architecture is core interest

Recent events:
many frontend design interactions this week

Correct resolution:
frontend spike likely project driven
backend remains core interest

Track both truths.
Do not overwrite explicit profile.

⸻

Memory Decay and Evolution

Not all memories should live forever.

Rules:

* stale weak memories decay over time
* contradictory evidence lowers confidence
* duplicates merge
* archived memories remain inspectable
* core profile memories persist until changed by user

⸻

Procedural Learning Engine

nUse confirmed patterns to improve workflows.

Examples:
If user repeatedly rates deep sessions highly:
Increase default technical depth.

If user ignores generic news:
Reduce broad news recommendations.

If user prefers architecture docs:
Prioritize diagrams and system views.

⸻

Frontend Memory Center

Create a dedicated memory UI.

Sections

Profile

* interests
* goals
* projects
* preferences
* skill levels

Learned About Me

* inferred memories
* confidence
* evidence
* approve / reject / edit

Timeline

* sessions
* saved content
* workflow outcomes
* feedback history

Rules

* workflow behavior preferences
* personalization settings

Graph View

* projects, topics, tools, interests, relationships

Review Queue

* new memory proposals
* conflicts
* stale memory cleanup suggestions

⸻

Agent Integration Rules

All agents should use Memory API.

Pattern:

1. ask for MemoryContext
2. perform task with context
3. emit events
4. emit candidate learnings
5. never self write long term memory directly

⸻

Recommended Tech Stack

.NET backend:

* Memory API
* typed models
* permissions
* auditability

Python workers:

* embeddings
* clustering
* summarization
* consolidation
* ranking

Temporal:

* nightly jobs
* periodic maintenance
* approval workflows

PostgreSQL:

* truth layer

pgvector initially:

* semantic retrieval

Neo4j later:

* graph memory

⸻

Roadmap

Phase 1

PostgreSQL memory domain.
Profile memory.
Event ingestion.
Basic retrieval.

Phase 2

Semantic memory engine.
Nightly consolidation.
Review queue.

Phase 3

Procedural learning.
Better ranking.
Context packets for all agents.

Phase 4

Graph memory.
Cross project reasoning.
Relationship exploration.

Phase 5

Advanced adaptive memory.
Predictive personalization.
Self optimizing workflows.

⸻

Non Negotiable Rules

1. User entered truth has highest authority.
2. Important memory changes require approval.
3. Every inferred memory needs evidence.
4. User can inspect why memory exists.
5. User can edit or delete memory.
6. Memory should reduce friction, not create creepiness.
7. Retrieval should be selective and relevant.
8. Privacy and trust are mandatory.

⸻

Success Criteria

After years of use, the platform should:

* know what matters to the user
* preserve project continuity across time
* personalize intelligently
* improve recommendations continuously
* reduce repetitive prompting
* feel like a real second brain
* remain trustworthy and controllable
* become more useful every month