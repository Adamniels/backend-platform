using System.Text.Json;
using FluentValidation;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.Application.Features.Memory.Events.IngestEvent;

public sealed class IngestMemoryEventCommandValidator : AbstractValidator<IngestMemoryEventCommand>
{
    public IngestMemoryEventCommandValidator()
    {
        var now = () => DateTimeOffset.UtcNow;

        RuleFor(x => x.EventType)
            .Must(et => !string.IsNullOrWhiteSpace(et))
            .WithMessage("EventType is required.")
            .MaximumLength(256);
        RuleFor(x => x.Domain)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.Domain));
        RuleFor(x => x.WorkflowId)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.WorkflowId));
        RuleFor(x => x.ProjectId)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.ProjectId));
        RuleFor(x => x.UserId)
            .Must(id => id == 0 || id == MemoryUser.DefaultId)
            .WithMessage("UserId must be omitted, 0, or 1 in the current deployment.");
        RuleFor(x => x.PayloadJson)
            .Must(BeValidJsonWhenPresent)
            .WithMessage("PayloadJson must be valid JSON when provided.");
        RuleFor(x => x.OccurredAt)
            .Must(occ => !occ.HasValue || occ.Value <= now().AddMinutes(1))
            .WithMessage("OccurredAt is too far in the future relative to the system clock.");
    }

    private static bool BeValidJsonWhenPresent(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            _ = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
