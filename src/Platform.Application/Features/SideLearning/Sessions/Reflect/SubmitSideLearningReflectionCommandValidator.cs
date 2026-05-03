using FluentValidation;

namespace Platform.Application.Features.SideLearning.Sessions.Reflect;

public sealed class SubmitSideLearningReflectionCommandValidator : AbstractValidator<SubmitSideLearningReflectionCommand>
{
    public SubmitSideLearningReflectionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Reflection).NotEmpty().MaximumLength(16384);
    }
}
