namespace Platform.Application.Features.News.RecordInteraction;

public sealed record RecordNewsInteractionCommand(
    int UserId,
    string NewsItemId,

    /// <summary>"read" | "save" | "dismiss" — validated and mapped to enum by the handler.</summary>
    string Type,

    /// <summary>Required when Type is "read". Must be null for "save" and "dismiss".</summary>
    int? DwellSeconds);
