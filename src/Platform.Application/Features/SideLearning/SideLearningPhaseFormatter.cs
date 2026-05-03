using Platform.Domain.Features.SideLearning;

namespace Platform.Application.Features.SideLearning;

public static class SideLearningPhaseFormatter
{
    public static string ToApiString(SideLearningSessionPhase phase) =>
        phase switch
        {
            SideLearningSessionPhase.ProposingTopics => "proposingTopics",
            SideLearningSessionPhase.AwaitingTopicSelection => "awaitingTopicSelection",
            SideLearningSessionPhase.GeneratingSession => "generatingSession",
            SideLearningSessionPhase.SessionReady => "sessionReady",
            SideLearningSessionPhase.InProgress => "inProgress",
            SideLearningSessionPhase.AwaitingReflection => "awaitingReflection",
            SideLearningSessionPhase.AnalyzingReflection => "analyzingReflection",
            SideLearningSessionPhase.Completed => "completed",
            SideLearningSessionPhase.Failed => "failed",
            _ => "failed",
        };
}
