# API guidelines

Normative **HTTP** and transport conventions. Aligns with [backend-standards.md](backend-standards.md) and the **400 + structured validation** target for FluentValidation (implementation may be incremental).

## Stack

- **ASP.NET Core minimal APIs** — no controllers in this solution
- **Swagger** — enabled in **Development** only; see [Program.cs](../src/Platform.Api/Program.cs)

## Route layout

- **Versioned product API:** `MapGroup("/api/v1")` in [V1ApiRegistration.cs](../src/Platform.Api/Features/V1ApiRegistration.cs) (the group is the base path for all `*V1Routes` maps)
- **Admin / access:** `/api/admin` in [AdminAccessRoutes.cs](../src/Platform.Api/Features/Access/AdminAccessRoutes.cs)
- **Health / readiness:** `/health`, `/ready` in [Program.cs](../src/Platform.Api/Program.cs)

## JSON

- **Property naming:** camelCase via `ConfigureHttpJsonOptions` in [Program.cs](../src/Platform.Api/Program.cs) (`PropertyNamingPolicy`, `DictionaryKeyPolicy`).

## CORS

- Policy name: **`platform`**
- Configured in [Program.cs](../src/Platform.Api/Program.cs); `Platform:AllowedOrigins` when set

## Antiforgery

- JSON API POSTs that are not form posts use **`.DisableAntiforgery()`** where used today (e.g. workflow start, admin unlock) — follow the same for similar endpoints

## Rate limiting

- **Admin unlock:** `RequireRateLimiting("unlock")` on the route; policy defined in [AccessRateLimiting.AddUnlockRateLimiter](../src/Platform.Api/Features/Access/AccessRateLimiting.cs)

## Problem responses

- **Target:** `ProblemDetails` / RFC 7807 for validation and recoverable client errors, per [backend-standards.md](backend-standards.md) and [error-handling-and-validation.md](error-handling-and-validation.md)
- **Admin unlock** already maps business outcomes to 200/401/503 with problem-style responses in places — use as a pattern for explicit HTTP semantics

## Related

- [auth-and-security.md](auth-and-security.md)
- [error-handling-and-validation.md](error-handling-and-validation.md)
- [feature-development-guide.md](feature-development-guide.md)
