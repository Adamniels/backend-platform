using Platform.Application.Features.News.Embed;

namespace Platform.UnitTests;

public sealed class EmbedNewsItemCommandValidatorTests
{
    private readonly EmbedNewsItemCommandValidator _validator = new();

    [Fact]
    public void Valid_ni_id_passes()
    {
        var result = _validator.Validate(new EmbedNewsItemCommand("ni-" + new string('a', 32)));
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-valid")]
    [InlineData("ni-tooshort")]
    [InlineData("ni-" + "g" + "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa" + "a")] // 'g' is not hex
    public void Invalid_ids_fail_validation(string id)
    {
        var result = _validator.Validate(new EmbedNewsItemCommand(id));
        Assert.False(result.IsValid);
    }
}
