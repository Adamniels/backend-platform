namespace Platform.Application.Features.SideLearning.Sessions.List;

/// <param name="Lifecycle"><c>ongoing</c> or <c>archive</c> (lowercase recommended).</param>
public readonly record struct ListSideLearningSessionsQuery(string Lifecycle, int Take = 50);
