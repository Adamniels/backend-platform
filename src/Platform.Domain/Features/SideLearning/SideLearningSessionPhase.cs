namespace Platform.Domain.Features.SideLearning;

public enum SideLearningSessionPhase
{
    ProposingTopics = 0,
    AwaitingTopicSelection = 1,
    GeneratingSession = 2,
    SessionReady = 3,
    InProgress = 4,
    AwaitingReflection = 5,
    AnalyzingReflection = 6,
    Completed = 7,
    Failed = 8,
}
