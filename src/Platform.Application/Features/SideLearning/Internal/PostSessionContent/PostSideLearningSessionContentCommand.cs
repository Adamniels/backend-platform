using Platform.Contracts.V1.SideLearning;

namespace Platform.Application.Features.SideLearning.Internal.PostSessionContent;

public sealed record PostSideLearningSessionContentCommand(string SessionId, PostSideLearningSessionContentV1Request Body);
