using FluentValidation;

namespace Platform.Application.Features.SideLearning.Sessions.RefreshTopicProposals;

public sealed class RefreshSideLearningTopicProposalsCommandValidator : AbstractValidator<RefreshSideLearningTopicProposalsCommand>
{
    public RefreshSideLearningTopicProposalsCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Feedback).MaximumLength(4096);
    }
}
