namespace RagMcpServer.Services;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using RagMcpServer.Configuration;

public class OllamaEmbeddingService : ITextEmbeddingGenerationService
{
    private readonly HttpClient _client;
    private readonly string _modelName;

    public OllamaEmbeddingService(IOptions<AIConfig> config)
    {
        var settings = config.Value.TextEmbedding;
        var endpoint = !string.IsNullOrEmpty(settings.Endpoint) ? settings.Endpoint : "http://localhost:11434";
        _client = new HttpClient { BaseAddress = new Uri(endpoint) };
        _modelName = !string.IsNullOrEmpty(settings.ModelId) ? settings.ModelId : "nomic-embed-text";
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
            
            HttpResponseMessage? response = null;
            int maxRetries = 10;
            for (int i = 0; i < maxRetries; i++)
            {
                try 
                {
                    // Re-create content for each retry
                    content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    response = await _client.PostAsync("/api/embeddings", content, cancellationToken);
                    if (response.IsSuccessStatusCode) break;
                    
                    if ((int)response.StatusCode >= 500)
                    {
                        var delay = 2000 * (i + 1); // Aggressive backoff: 2s, 4s, 6s...
                        await Task.Delay(delay, cancellationToken); 
                        continue;
                    }
                    else 
                    {
                        break;
                    }
                }
                catch (HttpRequestException)
                {
                    if (i == maxRetries - 1) throw;
                    await Task.Delay(2000 * (i + 1), cancellationToken);
                }
            }

            // Throttle slightly to prevent overwhelming the runner
            await Task.Delay(300, cancellationToken);

            if (response == null || !response.IsSuccessStatusCode)
            {
                var body = response != null ? await response.Content.ReadAsStringAsync(cancellationToken) : "No response";
                
                if (body.Contains("EOF") || body.Contains("connection refused"))
                {
                     throw new InvalidOperationException(
                        $"Ollama appears to have crashed or is unreachable (Error: {body}). This is often caused by insufficient RAM/VRAM or the model crashing on a specific input. Try restarting Ollama or reducing the chunk size.");
                }

                throw new InvalidOperationException(
                    $"Ollama embeddings API failed after {maxRetries} retries. Status: {response?.StatusCode}. Body: {body}");
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
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
