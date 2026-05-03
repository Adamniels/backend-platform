using FluentValidation;

namespace Platform.Application.Features.SideLearning.Sessions.Delete;

public sealed class DeleteSideLearningSessionCommandValidator : AbstractValidator<DeleteSideLearningSessionCommand>
{
    public DeleteSideLearningSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(64);
    }
}
