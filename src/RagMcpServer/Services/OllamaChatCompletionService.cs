namespace RagMcpServer.Services;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Extensions.Options;
using RagMcpServer.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class OllamaChatCompletionService : IChatCompletionService
{
    private readonly HttpClient _client;
    private readonly string _modelName;

    public OllamaChatCompletionService(IOptions<AIConfig> config)
    {
        var settings = config.Value.TextGeneration;
        var endpoint = !string.IsNullOrEmpty(settings.Endpoint) ? settings.Endpoint : "http://localhost:11434";
        _client = new HttpClient { BaseAddress = new Uri(endpoint) };
        _modelName = !string.IsNullOrEmpty(settings.ModelId) ? settings.ModelId : "llama3.1";
        Attributes = new Dictionary<string, object?>() as IReadOnlyDictionary<string, object?>;
    }

    public IReadOnlyDictionary<string, object?> Attributes { get; }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Simple implementation: concatenate chat history into a single prompt for Ollama's /api/generate (or use /api/chat if preferred)
        // Using /api/chat is better for structured chat history.

        var messages = new List<object>();
        foreach (var msg in chatHistory)
        {
            messages.Add(new
            {
                role = msg.Role == AuthorRole.User ? "user" : (msg.Role == AuthorRole.System ? "system" : "assistant"),
                content = msg.Content
            });
        }

        var requestBody = new
        {
            model = _modelName,
            messages = messages,
            stream = false
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/chat", content, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Ollama chat API failed: {response.StatusCode}. Body: {responseBody}");
        }
        
        var chatResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseBody);

        return new List<ChatMessageContent>
        {
            new ChatMessageContent(AuthorRole.Assistant, chatResponse?.message?.content ?? string.Empty)
        };
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Streaming is not yet implemented.");
    }
}

public class OllamaChatResponse
{
    public OllamaChatMessage? message { get; set; }
}

public class OllamaChatMessage
{
    public string? role { get; set; }
    public string? content { get; set; }
}
