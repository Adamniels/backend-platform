# Naming conventions

Aligned with [backend-standards.md](backend-standards.md) (**read port** naming) and existing code.

## Projects and namespaces

- Prefix: `Platform.`
- Namespace matches folder, e.g. `Platform.Application.Features.WorkflowRuns.StartWorkflowRun`

## Application

| Item | Pattern | Example |
| --- | --- | --- |
| Command | `*Command` | `StartWorkflowRunCommand` |
| Query | `*Query` (often `record struct` when empty) | `GetDashboardSummaryQuery` |
| Handler | `*QueryHandler` / `*CommandHandler` | `StartWorkflowRunCommandHandler` |
| Validator | `AbstractValidator<T>` in `*Validator` class | `StartWorkflowRunCommandValidator` |
| Port (read, table-shaped) | `I*ReadRepository` | `INewsReadRepository` |
| Port (read, composed) | `I*ReadModelSource` | `IDashboardReadModelSource` |
| Port (read/write on aggregate) | `I*Repository` | `IWorkflowRunRepository` |
| Workflow / options | `IWorkflowStarter`, `IWorkflowStartOptions` | Abstractions in [Abstractions/Workflows](../src/Platform.Application/Abstractions/Workflows) |
| Application helper | Static `*Formatter` in feature `Shared/` | `WorkflowRunStatusFormatter` |

**New** ports: pick the name that best matches the **closest** existing feature in the same area; avoid inventing a new synonym without reason.

## Api

- Route class: `*V1Routes` for product; `AdminAccessRoutes` for `/api/admin`
- Registration: `MapV1Endpoints` on `IEndpointRouteBuilder` in [V1ApiRegistration](../src/Platform.Api/Features/V1ApiRegistration.cs)
- Access rate limit extension: [AccessRateLimiting](../src/Platform.Api/Features/Access/AccessRateLimiting.cs) (`AddUnlockRateLimiter`)

## Contracts

- v1: `*Dto` for response payloads, `*Request` for request bodies in [V1/](../src/Platform.Contracts/V1)
- Admin: `UnlockRequest`, `*Response` in [Admin/](../src/Platform.Contracts/Admin)
- Use `JsonPropertyName` when the JSON name must differ from the C# name

## Domain

- `PlatformProfile`, `NewsItem` — feature-oriented folder under [Features/](../src/Platform.Domain/Features)
- `WorkflowRunStatus` enum in domain; API string for JSON is formatted in [WorkflowRunStatusFormatter](../src/Platform.Application/Features/WorkflowRuns/Shared/WorkflowRunStatusFormatter.cs) (and Infrastructure list queries use a local mapper for EF projections—see [infrastructure-guide.md](infrastructure-guide.md) for the direction to **converge** on one approach over time if desired)

## Tests

- Assembly: `Platform.UnitTests` / `Platform.IntegrationTests`
- Method style: descriptive (`Unlock_with_wrong_key_returns_401` style is acceptable)

## Related

- [backend-standards.md](backend-standards.md)
- [feature-development-guide.md](feature-development-guide.md)
