using Platform.Application.Features.Memory.Events;
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
    public void User_id_is_not_validated_at_command_level()
    {
        var v = new IngestMemoryEventCommandValidator();
        var multiUser = v.Validate(
            new IngestMemoryEventCommand("evt", null, null, null, null, 2, null));
        Assert.True(multiUser.IsValid);

        var defaultUser = v.Validate(
            new IngestMemoryEventCommand("evt", null, null, null, null, MemoryUser.DefaultId, null));
        Assert.True(defaultUser.IsValid);
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

    [Fact]
    public void Payload_json_over_max_length_fails()
    {
        var v = new IngestMemoryEventCommandValidator();
        var huge = new string('x', MemoryEventPayloadLimits.MaxPayloadJsonChars + 1);
        var result = v.Validate(new IngestMemoryEventCommand("evt", null, null, null, huge, 0, null));
        Assert.False(result.IsValid);
    }
}
