namespace Platform.Application.Features.SideLearning.Sessions.SelectTopic;

public sealed record SelectSideLearningTopicCommand(string SessionId, string TopicTitle, string? Feedback);
