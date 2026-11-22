using OllamaSharp;

namespace RAGPdf.Services;

public class EmbeddingService
{
    private readonly OllamaApiClient _client;

    public EmbeddingService(string endpoint, string model)
    {
        _client = new OllamaApiClient(new Uri(endpoint), model);
    }

    public async Task<float[]> CreateEmbedding(string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var embedding = await _client.EmbedAsync(text, cancellationToken);

        return embedding.Embeddings[0].ToArray();
    }
}
