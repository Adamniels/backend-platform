namespace Platform.Contracts.V1;

public sealed record StatTileDto(string Label, int Value, string Unit, string Color, string Sub);
public sealed record StatProgressDto(string Label, int Value, string Color);
public sealed record StatActivityDto(string Day, int Sessions);
public sealed record StatsPayloadDto(
    IReadOnlyList<StatTileDto> Tiles,
    IReadOnlyList<StatProgressDto> Progress,
    IReadOnlyList<StatActivityDto> Activity);
