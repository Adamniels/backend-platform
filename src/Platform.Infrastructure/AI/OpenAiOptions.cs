namespace Platform.Infrastructure.AI;

public sealed class OpenAiOptions
{
    public const string SectionKey = "OpenAi";

    public string ApiKey { get; set; } = "";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>Model used for embeddings, e.g. "text-embedding-3-small" (1536 dims).</summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}
