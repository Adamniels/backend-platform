using FluentValidation;

namespace Platform.Application.Features.SideLearning.Sessions.SelectTopic;

public sealed class SelectSideLearningTopicCommandValidator : AbstractValidator<SelectSideLearningTopicCommand>
{
    public SelectSideLearningTopicCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TopicTitle).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Feedback).MaximumLength(4096);
    }
}
