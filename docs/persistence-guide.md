# Persistence guide

## Technology

- **EF Core** with **Npgsql** for PostgreSQL
- **Context:** [PlatformDbContext.cs](../src/Platform.Infrastructure/Persistence/PlatformDbContext.cs)

## Connection string

- `ConnectionStrings:Default` in configuration; a **fallback** to local dev credentials exists in [Infrastructure/DependencyInjection.cs](../src/Platform.Infrastructure/DependencyInjection.cs) (development convenience — production must use explicit config)

## Migrations

- Migrations: [Migrations/](../src/Platform.Infrastructure/Persistence/Migrations) next to the context
- **On startup** the API runs `Database.MigrateAsync()` in [Program.cs](../src/Platform.Api/Program.cs) in a scope for **dev and test** workflows
- **Production (normative):** Prefer running migrations in a **pipeline / job** (see [backend-standards.md](backend-standards.md)), not as the only form of control for schema rollout

## Seeding and singleton rows

- Development/demo **HasData** in `OnModelCreating` for several entities, plus singleton key patterns (profile, user settings, stats snapshot)

## Read models

- Simple `AsNoTracking()` + `Select` to DTOs is the usual read path; see [infrastructure-guide.md](infrastructure-guide.md)

## Related

- [domain-layer-guide.md](domain-layer-guide.md)
- [infrastructure-guide.md](infrastructure-guide.md)
