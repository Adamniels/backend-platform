namespace Platform.Application.Abstractions.Workflows;

public interface IWorkflowStartOptions
{
    string GetDefaultTaskQueue();
}
