using Platform.Application.Features.News.Profile;

namespace Platform.UnitTests;

public sealed class SeedNewsProfileCommandValidatorTests
{
    private readonly SeedNewsProfileCommandValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Positive_user_id_passes(int userId)
    {
        var result = _validator.Validate(new SeedNewsProfileCommand(userId));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_user_id_fails(int userId)
    {
        var result = _validator.Validate(new SeedNewsProfileCommand(userId));
        Assert.False(result.IsValid);
    }
}
