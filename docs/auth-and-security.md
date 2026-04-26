# Auth and security

## Product model (normative)

The **platform access gate** is the intended product model for protecting most HTTP routes: a **signed, HttpOnly cookie** issued after successful `POST /api/admin/unlock` with the configured access key. This is **not** a user-identity system. When the product later needs full **identity** (OIDC, users, roles), integrate with a design that keeps **Application** free of `HttpContext` and keeps session/cookie details in **Api** (see [backend-standards.md](backend-standards.md)).

## Components

| Piece | Location | Role |
| --- | --- | --- |
| Options | [PlatformAccessOptions](../src/Platform.Application/Configuration/PlatformAccessOptions.cs) | Section `Platform`, bind in [Program.cs](../src/Platform.Api/Program.cs) |
| Session / cookie | [PlatformAccessSessionService](../src/Platform.Api/Access/PlatformAccessSessionService.cs) | Data protection, issue/clear cookie, `TryValidateRequest` |
| Middleware | [RequirePlatformAccessMiddleware](../src/Platform.Api/Middleware/RequirePlatformAccessMiddleware.cs) | `401` if no valid session (with explicit bypasses) |
| Key validation | [AccessKeyValidationService](../src/Platform.Infrastructure/Access/AccessKeyValidationService.cs) | Outcome for unlock; **no** raw key logging |
| Unlock use case | [UnlockSessionCommandHandler](../src/Platform.Application/Features/Access/UnlockSession/UnlockSessionCommandHandler.cs) + [AdminAccessRoutes](../src/Platform.Api/Features/Access/AdminAccessRoutes.cs) | Handler returns outcome; route issues cookie and maps to HTTP |

## Middleware bypass (no session)

- `OPTIONS` always allowed
- `POST /api/admin/unlock`, `POST /api/admin/lock`
- If `Platform:PublicHealth` is true: `GET /health`, `GET /ready`
- **Development:** paths under `/swagger`

## Rate limit

- **Unlock** uses the `unlock` policy from [AccessRateLimiting](../src/Platform.Api/Features/Access/AccessRateLimiting.cs)

## Related

- [api-guidelines.md](api-guidelines.md)
- [error-handling-and-validation.md](error-handling-and-validation.md)
- [backend-standards.md](backend-standards.md)
