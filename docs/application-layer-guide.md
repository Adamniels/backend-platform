# Application layer guide

**Project:** [Platform.Application](../src/Platform.Application)

## Purpose

- **Use cases** as **concrete** `*QueryHandler` and `*CommandHandler` types
- **Input models:** `*Query` / `*Command` (records or record structs)
- **Ports:** interfaces in [Abstractions/](../src/Platform.Application/Abstractions) — no EF, no `HttpContext`
- **Validation:** [FluentValidation](https://www.fluentvalidation.net/) `AbstractValidator<T>`, registered in [Application/DependencyInjection.cs](../src/Platform.Application/DependencyInjection.cs) via `AddValidatorsFromAssembly`
- **Target** for validation errors: **400** with **structured** details ([backend-standards.md](backend-standards.md)); handlers may use `ValidateAndThrowAsync` until the host maps `ValidationException` to that contract

## Handlers

- **Scoped** registration, one class per use case
- Injected as **concrete** types in minimal API delegates (e.g. `GetDashboardSummaryQueryHandler`)
- **Public** entry point: `HandleAsync` (naming in existing code)

## Abstractions (ports)

- Grouped by feature area: `Access/`, `Dashboard/`, `Profile/`, `WorkflowRuns/`, `Workflows/` (for Temporal and options), etc.
- **IWorkflowStarter** and **IWorkflowStartOptions** isolate Temporal and configuration from handler logic

## Access outcome (not HTTP)

- [UnlockSessionCommandHandler](../src/Platform.Application/Features/Access/UnlockSession/UnlockSessionCommandHandler.cs) returns `UnlockSessionOutcome` — the **Api** issues cookies; see [auth-and-security.md](auth-and-security.md)

## Shared

- [WorkflowRunStatusFormatter](../src/Platform.Application/Features/WorkflowRuns/Shared/WorkflowRunStatusFormatter.cs) — maps domain `WorkflowRunStatus` to the string the API returns in DTOs (see [naming-conventions.md](naming-conventions.md) for alignment with Infrastructure mappings)

## Configuration in Application

- [PlatformAccessOptions](../src/Platform.Application/Configuration/PlatformAccessOptions.cs) — options type shared for binding in Api and for validation in Infrastructure, without depending on the host

## Related

- [infrastructure-guide.md](infrastructure-guide.md)
- [error-handling-and-validation.md](error-handling-and-validation.md)
- [backend-standards.md](backend-standards.md)
