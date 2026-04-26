using Microsoft.Extensions.Configuration;
using Platform.Application.Abstractions.Workflows;

namespace Platform.Infrastructure.Configuration;

public sealed class WorkflowStartOptions(IConfiguration configuration) : IWorkflowStartOptions
{
    public string GetDefaultTaskQueue()
    {
        var v = configuration["Temporal:DefaultTaskQueue"];
        return string.IsNullOrWhiteSpace(v) ? "platform" : v;
    }
}
