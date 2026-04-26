# Feature development guide

Use this checklist for a **new v1 (or admin) feature** in line with [backend-standards.md](backend-standards.md).

## 1. Wire model

- [ ] Add or extend types in [Platform.Contracts](../src/Platform.Contracts) (v1 under `V1/`, admin under `Admin/`)
- [ ] Rely on global **camelCase** JSON from [Program.cs](../src/Platform.Api/Program.cs) unless you set `JsonPropertyName`

## 2. Domain (if new storage)

- [ ] Add entity/enum under [Platform.Domain/Features/…](../src/Platform.Domain/Features)
- [ ] Add `DbSet` and configuration in [PlatformDbContext](../src/Platform.Infrastructure/Persistence/PlatformDbContext.cs) and add an EF **migration** if the schema changes

## 3. Port (Application)

- [ ] Add interface under [Platform.Application/Abstractions/…](../src/Platform.Application/Abstractions) (name per [naming-conventions.md](naming-conventions.md))

## 4. Implementation (Infrastructure)

- [ ] Implement the port in [Platform.Infrastructure/Features/…](../src/Platform.Infrastructure/Features) (or [Access/](../src/Platform.Infrastructure/Access), [Temporal/](../src/Platform.Infrastructure/Temporal) as appropriate)
- [ ] Register: `AddScoped<Interface, Implementation>()` in [Infrastructure/DependencyInjection.cs](../src/Platform.Infrastructure/DependencyInjection.cs)
- [ ] For **simple reads:** project to contract DTOs in EF if appropriate ([backend-standards.md](backend-standards.md))

## 5. Use case (Application)

- [ ] `*Query` / `*Command` in `Features/<Feature>/<UseCase>/`
- [ ] `*Handler` with `HandleAsync` (or equivalent) returning `Platform.Contracts` types
- [ ] `AbstractValidator<T>` if validation is non-trivial; register via Application assembly (already in [AddPlatformApplication](../src/Platform.Application/DependencyInjection.cs))
- [ ] Register handler: `AddScoped<YourHandler>()` in [Application/DependencyInjection.cs](../src/Platform.Application/DependencyInjection.cs)

## 6. HTTP (Api)

- [ ] Add or update `*V1Routes` under [Platform.Api/Features/…](../src/Platform.Api/Features)
- [ ] Call `Map(…)` from [V1ApiRegistration.MapV1Endpoints](../src/Platform.Api/Features/V1ApiRegistration.cs) (or `MapAdminEndpoints` in [AdminAccessRoutes](../src/Platform.Api/Features/Access/AdminAccessRoutes.cs))
- [ ] Route delegate: build command/query, call **one** handler, return `Results.*`

## 7. Security

- [ ] If the path must be **public** without a session, update [RequirePlatformAccessMiddleware](../src/Platform.Api/Middleware/RequirePlatformAccessMiddleware.cs) **with intent**; default is 401 for protected areas

## 8. Tests

- [ ] Unit test for validators; handler tests with mocks for non-trivial commands ([testing-guide.md](testing-guide.md))
- [ ] Integration test for important flows and HTTP contracts

## PR copy-paste checklist

```
Contracts (if new shapes)
Domain + migration (if new schema)
I* port + Infra implementation + both DI
Command/Query + Handler + Validator? + App DI
*V1Routes + V1ApiRegistration (or Admin)
Middleware bypass only if product-approved
Unit + integration tests
```

## Related

- [api-guidelines.md](api-guidelines.md)
- [ai-agent-guide.md](ai-agent-guide.md)
- [persistence-guide.md](persistence-guide.md)
