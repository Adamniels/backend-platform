# Backend standards (normative)

This document is the **authoritative ruleset** for how `backend-platform` should be built and extended. It combines **explicit project decisions** with stable patterns already in the codebase.

## Project decisions

| Topic | Decision |
| --- | --- |
| **DTO / mapping (reads)** | Infrastructure **may** project to `Platform.Contracts` DTOs in EF (e.g. `Select(x => new SomeDto(...))`) for **simple** read paths. When a use case needs domain rules or non-trivial transforms, the **Application** handler maps from domain or internal read models to contract DTOs. |
| **Validation + HTTP** | **Target:** FluentValidation failures should surface as **400 Bad Request** with **structured** validation details (use **ProblemDetails** with extensions or an equivalent RFC 7807-friendly shape for field errors). The host may not yet implement this end-to-end; new work should move the API toward this target. |
| **Migrations (prod vs dev)** | **Dev / test:** Applying migrations on API startup (as in [Program.cs](../src/Platform.Api/Program.cs)) is acceptable. **Production:** Migrations should run in a **deploy pipeline or dedicated job**, not rely on “every instance on startup” unless the team explicitly chooses that operational model. Document the pipeline in runbooks, not only in code. |
| **Architecture style** | **No MediatR.** Use **direct DI** of **concrete** `*QueryHandler` / `*CommandHandler` types from minimal API route delegates. **Clean Architecture** layering: Api → Application + Infrastructure; Application → Domain + Contracts; Infrastructure → Application + Domain. |
| **Wire types** | All public JSON request/response models live in **Platform.Contracts** (`*Request`, `*Dto`, admin `*Response` as needed). **camelCase** JSON (see [Program.cs](../src/Platform.Api/Program.cs)). |
| **Ports** | Application defines **abstractions** under [Platform.Application/Abstractions](../src/Platform.Application/Abstractions). Infrastructure **implements** them. Application **must not** reference `DbContext`, EF, or `HttpContext`. |
| **Validation** | **FluentValidation** in Application: `AbstractValidator<T>`, register via `AddValidatorsFromAssembly` in [Application DependencyInjection](../src/Platform.Application/DependencyInjection.cs). Validators are invoked from handlers (e.g. `ValidateAndThrowAsync`) unless the team later introduces a single pipeline. |
| **Read port naming** | Prefer **feature-clear** names. Use `I*ReadRepository` for table-shaped list/detail reads. Use a **composed** name such as `I*ReadModelSource` (or `IDashboardReadModelSource`) when multiple tables are aggregated. New ports should follow the **closest** existing feature in the same area. |
| **Auth (product model)** | The current **platform access cookie** after `POST /api/admin/unlock` is the **intended gating** model for this product until a separate identity system is introduced. New auth mechanisms should integrate **without** blurring `HttpContext` into Application (keep cookie/session wiring in **Api**). |
| **Testing (target)** | For **new commands** and non-trivial handlers: add **unit** tests with **mocked** ports where practical, plus **integration** tests for critical HTTP flows. **Validators** get unit tests. Exact coverage is a PR judgment call; the **direction** is protect regressions on orchestration. |
| **Logging (target)** | **Direction:** use structured logging; add **request correlation** (e.g. request id) in middleware when the team adds the plumbing. No mandatory OpenTelemetry in this document until adopted. |

## Do

- Add `AddPlatformApplication()` **before** `AddPlatformInfrastructure(...)` in the host (see [Program.cs](../src/Platform.Api/Program.cs)).
- Register new handlers in [Application/DependencyInjection.cs](../src/Platform.Application/DependencyInjection.cs) and new port implementations in [Infrastructure/DependencyInjection.cs](../src/Platform.Infrastructure/DependencyInjection.cs).
- Add v1 routes under [Platform.Api/Features](../src/Platform.Api/Features) and register them in [V1ApiRegistration.cs](../src/Platform.Api/Features/V1ApiRegistration.cs).
- Keep **unlock** and session **cookie** concerns in the **Api** layer; Application returns outcomes (e.g. `UnlockSessionOutcome`).

## Don’t

- Don’t put **EF** or **DbContext** in Application or Domain.
- Don’t add **MediatR** (or a dynamic mediator) without an explicit design review.
- Don’t return **sensitive** values in logs (e.g. raw access keys).

## Related

- [architecture.md](architecture.md)
- [naming-conventions.md](naming-conventions.md)
- [error-handling-and-validation.md](error-handling-and-validation.md)
- [persistence-guide.md](persistence-guide.md)
