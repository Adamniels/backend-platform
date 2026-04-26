# Infrastructure guide

**Project:** [Platform.Infrastructure](../src/Platform.Infrastructure)

## Purpose

- **Implement** Application [Abstractions/](../src/Platform.Application/Abstractions)
- Host **EF Core** [PlatformDbContext](../src/Platform.Infrastructure/Persistence/PlatformDbContext.cs)
- **Temporal** [TemporalWorkflowStarter](../src/Platform.Infrastructure/Temporal/TemporalWorkflowStarter.cs) (or [StubWorkflowStarter](../src/Platform.Infrastructure/Temporal/StubWorkflowStarter.cs) when not configured)
- [AccessKeyValidationService](../src/Platform.Infrastructure/Access/AccessKeyValidationService.cs) for static access key check (no raw key logging)
- [WorkflowStartOptions](../src/Platform.Infrastructure/Configuration/WorkflowStartOptions.cs) for default task queue from `IConfiguration`

## Dependency injection

- [AddPlatformInfrastructure](../src/Platform.Infrastructure/DependencyInjection.cs) registers DbContext, ports, and Temporal. Run **after** [AddPlatformApplication](../src/Platform.Application/DependencyInjection.cs) in the host ([Program.cs](../src/Platform.Api/Program.cs))

## DTO mapping (normative)

Per [backend-standards.md](backend-standards.md), you **may** project to **Platform.Contracts** DTOs in EF for **simple** read queries (e.g. [NewsReadRepository](../src/Platform.Infrastructure/Features/News/NewsReadRepository.cs)). For more complex use cases, return domain or intermediate shapes and let the **Application** handler map to contracts.

## Workflow run status display strings

- [WorkflowRunStatusMapper](../src/Platform.Infrastructure/Features/WorkflowRuns/WorkflowRunStatusMapper.cs) is used in list projections. Application uses [WorkflowRunStatusFormatter](../src/Platform.Application/Features/WorkflowRuns/Shared/WorkflowRunStatusFormatter.cs) for the start handler path. **Direction over time:** consolidate to a single string convention (same strings in both) or share one helper via a design that does not break layering—avoid drift in meaning.

## Temporal

- `Temporal:Address` absent → stub; set → [TemporalWorkflowStarter](../src/Platform.Infrastructure/Temporal/TemporalWorkflowStarter.cs) registers as `IWorkflowStarter`

## Related

- [persistence-guide.md](persistence-guide.md)
- [application-layer-guide.md](application-layer-guide.md)
- [auth-and-security.md](auth-and-security.md)
