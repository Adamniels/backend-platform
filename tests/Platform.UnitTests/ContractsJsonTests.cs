using System.Text.Json;
using Platform.Contracts.Admin;

namespace Platform.UnitTests;

public sealed class ContractsJsonTests
{
    [Fact]
    public void UnlockRequest_deserializes_camelCase_accessKey()
    {
        var json = """{"accessKey":"secret-value"}""";
        var o = JsonSerializer.Deserialize<UnlockRequest>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(o);
        Assert.Equal("secret-value", o.AccessKey);
    }
}
