namespace Platform.Contracts.V1;

public sealed record InputNeededItemDto(int Id, string Text, string Type, bool Urgent, string Detail);
