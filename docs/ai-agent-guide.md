# AI agent guide

Use this file to **route** work to the right document and to avoid breaking **layer rules** in [backend-standards.md](backend-standards.md).

## Read first

1. [backend-standards.md](backend-standards.md) — **non-negotiable** rules and project decisions (DTO mapping, validation HTTP target, migrations in prod, etc.).
2. [architecture.md](architecture.md) — dependency direction and where code lives.
3. The specific file you are editing (handler, route, `PlatformDbContext`, `DependencyInjection`).

## Task → document

| Task | Read |
| --- | --- |
| Add a new v1 endpoint | [feature-development-guide.md](feature-development-guide.md), [api-guidelines.md](api-guidelines.md), [application-layer-guide.md](application-layer-guide.md) |
| Add DB entity + migration | [persistence-guide.md](persistence-guide.md), [domain-layer-guide.md](domain-layer-guide.md) |
| Implement a port | [infrastructure-guide.md](infrastructure-guide.md) |
| Change error/validation behavior | [error-handling-and-validation.md](error-handling-and-validation.md) (align with 400 + ProblemDetails **target** in standards) |
| Change auth / public routes | [auth-and-security.md](auth-and-security.md), [api-guidelines.md](api-guidelines.md) — update [RequirePlatformAccessMiddleware](../src/Platform.Api/Middleware/RequirePlatformAccessMiddleware.cs) only with clear product intent |
| Add logging / correlation | [logging-and-observability.md](logging-and-observability.md) |
| Add or fix tests | [testing-guide.md](testing-guide.md) |

## Safe patterns

- **Handlers** are concrete, **scoped**, registered in [Application/DependencyInjection.cs](../src/Platform.Application/DependencyInjection.cs). Inject them in route delegates, not a mediator.
- **Ports** are interfaces in `Platform.Application/Abstractions`; **implement** in `Platform.Infrastructure` and register in [Infrastructure/DependencyInjection.cs](../src/Platform.Infrastructure/DependencyInjection.cs).
- **HTTP** stays in `Platform.Api` (including cookie issuance for unlock). Application returns outcomes, not `HttpContext`.
- **Simple read lists:** Infrastructure may `Select` to contract DTOs (per [backend-standards.md](backend-standards.md)); use handler mapping when logic is non-trivial.

## Anti-patterns

- `DbContext` or EF types in Application or Domain.
- `MediatR` or indirection that hides the concrete handler (unless the team rewrites the standard).
- New **public** routes that skip [RequirePlatformAccessMiddleware](../src/Platform.Api/Middleware/RequirePlatformAccessMiddleware.cs) **without** updating `ShouldBypass` and security review.

## Verify locally

```bash
cd backend-platform
dotnet build
dotnet test
```

## Related

- [backend-standards.md](backend-standards.md)
- [feature-development-guide.md](feature-development-guide.md)
- [testing-guide.md](testing-guide.md)
