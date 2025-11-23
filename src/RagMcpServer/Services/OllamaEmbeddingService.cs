namespace RagMcpServer.Services;

using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

public class OllamaEmbeddingService : ITextEmbeddingGenerationService
{
    private readonly HttpClient _client;
    private readonly string _modelName;

    public OllamaEmbeddingService(IConfiguration configuration, string modelName = "nomic-embed-text")
    {
        var endpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        _client = new HttpClient { BaseAddress = new Uri(endpoint) };
        _modelName = modelName;
        Attributes = new Dictionary<string, object>() as IReadOnlyDictionary<string, object?>;
    }

    public IReadOnlyDictionary<string, object?> Attributes { get; }

    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var embeddings = new List<ReadOnlyMemory<float>>();
        foreach (var text in data)
        {
            var requestBody = new { model = _modelName, prompt = text };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/embeddings", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Ollama embeddings API failed with status code {(int)response.StatusCode} ({response.StatusCode}). " +
                    $"Body: {responseBody}");
            }

            var embeddingResponse = JsonSerializer.Deserialize<OllamaEmbeddingResponse>(responseBody);
            if (embeddingResponse?.embedding != null)
            {
                embeddings.Add(new ReadOnlyMemory<float>(embeddingResponse.embedding));
            }
        }
        return embeddings;
    }
}

public class OllamaEmbeddingResponse
{
    public float[]? embedding { get; set; }
}
