using FluentValidation;
using Platform.Application.Abstractions.Memory.Documents;

namespace Platform.Application.Features.Memory.Documents.IngestDocumentMemory;

public sealed class IngestDocumentMemoryCommandValidator : AbstractValidator<IngestDocumentMemoryCommand>
{
    public const int MaxContentLength = 1_000_000;

    public IngestDocumentMemoryCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(512);
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(MaxContentLength);
        RuleFor(x => x.SourceType)
            .MaximumLength(64)
            .When(x => !string.IsNullOrEmpty(x.SourceType));
        RuleFor(x => x.ProjectId)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.ProjectId));
        RuleFor(x => x.Domain)
            .MaximumLength(256)
            .When(x => !string.IsNullOrEmpty(x.Domain));
    }
}
