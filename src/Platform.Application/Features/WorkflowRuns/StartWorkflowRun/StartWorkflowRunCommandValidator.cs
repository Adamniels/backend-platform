using FluentValidation;

namespace Platform.Application.Features.WorkflowRuns.StartWorkflowRun;

public sealed class StartWorkflowRunCommandValidator : AbstractValidator<StartWorkflowRunCommand>
{
    public StartWorkflowRunCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.");
        RuleFor(x => x.WorkflowType)
            .NotEmpty()
            .WithMessage("WorkflowType is required.");
    }
}
