using Platform.Contracts.V1.SideLearning;

namespace Platform.Application.Features.SideLearning.Internal.PostTopicProposals;

public sealed record PostSideLearningTopicProposalsCommand(string SessionId, PostSideLearningTopicProposalsV1Request Body);
