using FluentValidation;

namespace Platform.Application.Features.SideLearning.Sessions.Progress;

public sealed class UpdateSideLearningProgressCommandValidator : AbstractValidator<UpdateSideLearningProgressCommand>
{
    public UpdateSideLearningProgressCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.SectionId).NotEmpty().MaximumLength(64);
    }
}
