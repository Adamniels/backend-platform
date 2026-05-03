namespace Platform.Application.Features.SideLearning.Sessions.Progress;

public sealed record UpdateSideLearningProgressCommand(string SessionId, string SectionId, bool Completed);
