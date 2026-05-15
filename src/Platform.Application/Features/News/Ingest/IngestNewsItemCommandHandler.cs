using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Platform.Application.Abstractions.News;
using Platform.Contracts.V1.News;
using Platform.Domain.Features.News;

namespace Platform.Application.Features.News.Ingest;

public sealed class IngestNewsItemCommandHandler(
    IValidator<IngestNewsItemCommand> validator,
    INewsIngestRepository ingest)
{
    public async Task<IngestNewsItemV1Response> HandleAsync(
        IngestNewsItemCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken).ConfigureAwait(false);

        // PublishedAt is guaranteed valid by the validator above.
        var publishedAt = DateTimeOffset.Parse(
            command.PublishedAt,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind);

        var normalizedUrl = command.Url.Trim();
        var urlHash = ComputeUrlHash(normalizedUrl);

        var id = $"ni-{Guid.NewGuid():N}";
        var item = new NewsItem
        {
            Id = id,
            Title = command.Title.Trim(),
            Url = normalizedUrl,
            UrlHash = urlHash,
            Source = command.Source.Trim(),
            Body = command.Body,
            Author = string.IsNullOrWhiteSpace(command.Author) ? null : command.Author.Trim(),
            PublishedAt = publishedAt,
            SourceFeedUrl = string.IsNullOrWhiteSpace(command.SourceFeedUrl)
                ? null
                : command.SourceFeedUrl.Trim(),
        };

        var (created, returnedId) = await ingest
            .TryInsertAsync(item, urlHash, cancellationToken)
            .ConfigureAwait(false);

        return created
            ? new IngestNewsItemV1Response("created", returnedId)
            : new IngestNewsItemV1Response("duplicate", null);
    }

    internal static string ComputeUrlHash(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url.ToLowerInvariant()));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
