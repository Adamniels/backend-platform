# Domain layer guide

**Project:** [Platform.Domain](../src/Platform.Domain)

## Purpose

- **Entities** and **enums** for persisted and business concepts
- **No** references to Application, Infrastructure, Api, or Contracts
- In practice entities are **EF Core–mapped** POCOs; behavior is not yet a rich domain model in most features

## Layout

- [Features/](../src/Platform.Domain/Features) — e.g. `WorkflowRuns/`, `Profile/`, `News/`

## Seeding and configuration

- **HasData** and property limits are applied in [PlatformDbContext.OnModelCreating](../src/Platform.Infrastructure/Persistence/PlatformDbContext.cs) in Infrastructure (persistence concern), not in Domain, which keeps Domain as plain types

## Enums and API strings

- e.g. `WorkflowRunStatus` in Domain; **string** for JSON in contracts is the responsibility of **formatters** in Application and/or list projections in Infrastructure, per [backend-standards.md](backend-standards.md) (favor a **single** formatting approach when refactoring)

## If introducing richer domain

- If you add invariants, prefer methods on the entity or small value objects, and keep Application handlers as orchestration — document any new pattern in [backend-standards.md](backend-standards.md)

## Related

- [persistence-guide.md](persistence-guide.md)
- [application-layer-guide.md](application-layer-guide.md)
