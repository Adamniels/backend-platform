using FluentValidation;

namespace Platform.Application.Features.SideLearning.Sessions.Create;

public sealed class CreateSideLearningSessionCommandValidator : AbstractValidator<CreateSideLearningSessionCommand>
{
    public CreateSideLearningSessionCommandValidator()
    {
        RuleFor(x => x.InitialPrompt).MaximumLength(4096);
    }
}
