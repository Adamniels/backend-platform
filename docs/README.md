# Platform backend documentation

Authoritative guidance for the **backend-platform** .NET solution. Start with **[backend-standards.md](backend-standards.md)** for non-negotiable rules and project decisions, then use the topic guides below.

## Who this is for

- Engineers shipping features in this repository
- AI agents that need correct layer boundaries and file locations (see [ai-agent-guide.md](ai-agent-guide.md))

## Solution map

| Project | Role |
| --- | --- |
| [Platform.Api](../src/Platform.Api) | HTTP host, minimal API routes, middleware, cookie transport for access |
| [Platform.Application](../src/Platform.Application) | Use cases: handlers, commands/queries, ports (interfaces), validators |
| [Platform.Domain](../src/Platform.Domain) | Entities and enums (no I/O) |
| [Platform.Infrastructure](../src/Platform.Infrastructure) | EF Core, port implementations, Temporal, access key validation |
| [Platform.Contracts](../src/Platform.Contracts) | JSON request/response DTOs |

**MediatR is not used** — see [backend-standards.md](backend-standards.md).

## Table of contents

| Document | Description |
| --- | --- |
| [backend-standards.md](backend-standards.md) | **Normative** rules and project decisions |
| [architecture.md](architecture.md) | Layers, dependencies, request flow |
| [naming-conventions.md](naming-conventions.md) | Names for projects, types, and folders |
| [feature-development-guide.md](feature-development-guide.md) | Checklist to add a feature end-to-end |
| [api-guidelines.md](api-guidelines.md) | Routes, JSON, CORS, Swagger, rate limits |
| [application-layer-guide.md](application-layer-guide.md) | Handlers, commands, validators, abstractions |
| [domain-layer-guide.md](domain-layer-guide.md) | Domain model and seeding |
| [infrastructure-guide.md](infrastructure-guide.md) | Implementing ports, EF, Temporal |
| [persistence-guide.md](persistence-guide.md) | DbContext, migrations (dev vs prod) |
| [auth-and-security.md](auth-and-security.md) | Access cookie, unlock, middleware bypasses |
| [error-handling-and-validation.md](error-handling-and-validation.md) | Exceptions, validation, 400/500 targets |
| [logging-and-observability.md](logging-and-observability.md) | Logging direction and gaps |
| [testing-guide.md](testing-guide.md) | Unit vs integration, how to run tests |
| [ai-agent-guide.md](ai-agent-guide.md) | Task → doc routing and safe change patterns |

## Operations (run, Postgres, env)

See the repository [README.md](../README.md) for Docker, `dotnet run`, and `.env` loading.
