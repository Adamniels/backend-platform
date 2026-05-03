using Platform.Contracts.V1.SideLearning;

namespace Platform.Application.Features.SideLearning.Internal.PostReflectionInsights;

public sealed record PostSideLearningReflectionInsightsCommand(string SessionId, PostSideLearningReflectionInsightsV1Request Body);
