# Error handling and validation

## Normative target

Per [backend-standards.md](backend-standards.md):

- **FluentValidation** failures should become **400 Bad Request** with **structured** validation information (ProblemDetails with extensions or equivalent field-level detail).
- **Global** unhandled exceptions should not leak internal details in production; use a **generic** message in the public response.

**Implementation path:** The host’s exception pipeline may need to **special-case** `ValidationException` to return 400 with a standard body. New work should move the API toward the target.

## Current host behavior (factual)

- [Program.cs](../src/Platform.Api/Program.cs) `UseExceptionHandler` returns a generic `Results.Problem` for unhandled exceptions, which may include `ValidationException` if not handled earlier — **treat as temporary** until 400 mapping exists.

## Handlers

- [StartWorkflowRunCommandHandler](../src/Platform.Application/Features/WorkflowRuns/StartWorkflowRun/StartWorkflowRunCommandHandler.cs) uses `ValidateAndThrowAsync` for input rules.

## Explicit business HTTP mapping

- [AdminAccessRoutes unlock](../src/Platform.Api/Features/Access/AdminAccessRoutes.cs) maps `UnlockSessionOutcome` to **503** (not configured), **401** (invalid key), **200** (success) — a model for **explicit** status choice where the product requires it.

## Related

- [api-guidelines.md](api-guidelines.md)
- [application-layer-guide.md](application-layer-guide.md)
- [backend-standards.md](backend-standards.md)
