namespace Platform.Application.Features.News.DeleteItems;

public sealed record DeleteNewsItemsCommand(IReadOnlyList<string> Ids);
