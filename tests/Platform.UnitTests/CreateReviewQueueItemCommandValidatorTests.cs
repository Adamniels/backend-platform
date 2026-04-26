using Platform.Application.Features.Memory.ReviewQueue.CreateItem;

namespace Platform.UnitTests;

public sealed class CreateReviewQueueItemCommandValidatorTests
{
    [Fact]
    public void New_semantic_requires_valid_json()
    {
        var v = new CreateReviewQueueItemCommandValidator();
        var bad = v.Validate(
            new CreateReviewQueueItemCommand(
                0,
                "NewSemantic",
                "t",
                "s",
                "{",
                null,
                1));
        Assert.False(bad.IsValid);

        var good = v.Validate(
            new CreateReviewQueueItemCommand(
                0,
                "NewSemantic",
                "t",
                "s",
                """{"kind":"NewSemantic","key":"k1","claim":"c1","initialConfidence":0.7}""",
                null,
                1));
        Assert.True(good.IsValid);
    }

    [Fact]
    public void New_procedural_requires_valid_json()
    {
        var v = new CreateReviewQueueItemCommandValidator();
        var bad = v.Validate(
            new CreateReviewQueueItemCommand(
                0,
                "NewProceduralRule",
                "t",
                "s",
                "{",
                null,
                1));
        Assert.False(bad.IsValid);

        var good = v.Validate(
            new CreateReviewQueueItemCommand(
                0,
                "NewProceduralRule",
                "t",
                "s",
                """{"kind":"NewProceduralRule","workflowType":"w","ruleName":"n","ruleContent":"c","priority":0,"source":"s","authorityWeight":0.55}""",
                null,
                1));
        Assert.True(good.IsValid);
    }
}
