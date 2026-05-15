using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Abstractions.Memory.Embeddings;
using Platform.Application.Features.Memory.Embeddings;

namespace Platform.Infrastructure.AI;

/// <summary>
/// Production embedding generator that calls the OpenAI /v1/embeddings endpoint.
/// Implements IMemoryEmbeddingGenerator so it is shared between news ranking and
/// the memory vector recall pipeline without duplication.
/// </summary>
public sealed class OpenAiEmbeddingGenerator : IMemoryEmbeddingGenerator
{
    private readonly OpenAiOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenAiEmbeddingGenerator> _logger;

    public OpenAiEmbeddingGenerator(
        IOptions<OpenAiOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiEmbeddingGenerator> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>Stable model key written to embedding rows — matches the model name for traceability.</summary>
    public string ModelKey => _options.EmbeddingModel;

    /// <summary>1536 for text-embedding-3-small.</summary>
    public int Dimensions => MemoryVectorRecallConstants.EmbeddingDimensions;

    public async Task<float[]?> TryEmbedRecallQueryAsync(
        string? text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured — embedding skipped.");
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient();
            var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/embeddings";

            var requestBody = new
            {
                input = text,
                model = _options.EmbeddingModel,
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(requestBody),
            };
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var response = await client
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OpenAI embeddings API returned {StatusCode} — embedding skipped.",
                    (int)response.StatusCode);
                return null;
            }

            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            using var doc = await JsonDocument
                .ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var embeddingArray = doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding");

            var result = new float[Dimensions];
            var i = 0;
            foreach (var element in embeddingArray.EnumerateArray())
            {
                if (i >= result.Length)
                    break;
                result[i++] = element.GetSingle();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to call OpenAI embeddings API — returning null.");
            return null;
        }
    }
}
