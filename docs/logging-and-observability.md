# Logging and observability

## Normative direction

- Prefer **structured** logging with consistent **category** or **event** names
- **Target:** add **request correlation** (e.g. request id in middleware) when the team implements it
- **OpenTelemetry** and distributed tracing: **not** required in [backend-standards.md](backend-standards.md) until the team adopts them

## What exists in code today (factual)

- **Global** errors: logger name `Platform.Errors` in [Program.cs](../src/Platform.Api/Program.cs) exception path
- **Access:** `ILogger<RequirePlatformAccessMiddleware>` for denied requests; named logger `"Platform.Access"` in [AdminAccessRoutes](../src/Platform.Api/Features/Access/AdminAccessRoutes.cs) for unlock flow
- **Temporal:** [TemporalWorkflowStarter](../src/Platform.Infrastructure/Temporal/TemporalWorkflowStarter.cs) logs start success/failure

## Gaps vs target

- No **single** request-scoped id across all log lines in one request (add middleware + `ILogger` scope when ready)
- No default **OTel** wiring in the solution

## Related

- [error-handling-and-validation.md](error-handling-and-validation.md)
- [backend-standards.md](backend-standards.md)
