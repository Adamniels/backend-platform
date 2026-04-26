using Platform.Application.Features.Memory.Events.IngestEvent;
using Platform.Domain.Features.Memory.Entities;

namespace Platform.UnitTests;

public sealed class IngestMemoryEventCommandValidatorTests
{
    [Fact]
    public void Whitespace_event_type_fails()
    {
        var v = new IngestMemoryEventCommandValidator();
        var result = v.Validate(new IngestMemoryEventCommand("   "));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Event_type_too_long_fails()
    {
        var v = new IngestMemoryEventCommandValidator();
        var result = v.Validate(
            new IngestMemoryEventCommand(
                new string('x', 257),
                null,
                null,
                null,
                null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void User_id_must_be_0_1_or_default()
    {
        var v = new IngestMemoryEventCommandValidator();
        var bad = v.Validate(
            new IngestMemoryEventCommand("evt", null, null, null, null, 2, null));
        Assert.False(bad.IsValid);

        var good = v.Validate(
            new IngestMemoryEventCommand("evt", null, null, null, null, MemoryUser.DefaultId, null));
        Assert.True(good.IsValid);
    }

    [Fact]
    public void Invalid_payload_json_fails()
    {
        var v = new IngestMemoryEventCommandValidator();
        var result = v.Validate(
            new IngestMemoryEventCommand("evt", null, null, null, "not json", 0, null));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Valid_command_with_object_payload_passes()
    {
        var v = new IngestMemoryEventCommandValidator();
        var result = v.Validate(
            new IngestMemoryEventCommand("workflow.step", null, "wf-1", "proj-1", "{\"a\":1}", 0, null));
        Assert.True(result.IsValid);
    }
}
