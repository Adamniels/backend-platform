using FluentValidation;

namespace Platform.Application.Features.Memory.Semantic.UpdateSemanticMemoryConfidence;

public sealed class UpdateSemanticMemoryConfidenceCommandValidator
    : AbstractValidator<UpdateSemanticMemoryConfidenceCommand>
{
    public UpdateSemanticMemoryConfidenceCommandValidator() =>
        RuleFor(x => x.Confidence)
            .InclusiveBetween(0, 1);
}
