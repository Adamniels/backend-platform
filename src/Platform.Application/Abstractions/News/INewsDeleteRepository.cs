namespace Platform.Application.Abstractions.News;

public interface INewsDeleteRepository
{
    Task<int> DeleteByIdsAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken = default);
}
