# Testing guide

## Normative direction

- **New commands** and **non-trivial** handlers: prefer **unit** tests with **mocked** Application ports when behavior is easy to isolate, plus **integration** tests for important HTTP and database paths
- **Validators:** unit test with `AbstractValidator` directly (see [StartWorkflowRunCommandValidatorTests](../tests/Platform.UnitTests/StartWorkflowRunCommandValidatorTests.cs))
- **Contracts JSON:** [ContractsJsonTests](../tests/Platform.UnitTests/ContractsJsonTests.cs) for wire shape

## Projects

| Project | Path | Role |
| --- | --- | --- |
| Unit | [Platform.UnitTests](../tests/Platform.UnitTests) | Validators, small contract tests |
| Integration | [Platform.IntegrationTests](../tests/Platform.IntegrationTests) | [WebApplicationFactory<Program>](../tests/Platform.IntegrationTests/PlatformWebApplicationFactory.cs), full HTTP with cookies |

## Running

```bash
cd backend-platform
dotnet test
```

## Integration environment

- [PlatformWebApplicationFactory](../tests/Platform.IntegrationTests/PlatformWebApplicationFactory.cs) uses environment **Testing** and sets `Platform:AccessKey` for unlock tests
- **Postgres** at the configured connection string may be required for tests that hit `/ready` and v1 data

## Related

- [feature-development-guide.md](feature-development-guide.md)
- [backend-standards.md](backend-standards.md)
- [ai-agent-guide.md](ai-agent-guide.md)
