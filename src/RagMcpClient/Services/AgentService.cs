using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RagMcpClient.Mcp;

namespace RagMcpClient.Services;

public class AgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly ILogger<AgentService>? _logger;

    public AgentService(IConfiguration config, ILogger<AgentService>? logger = null)
    {
        _logger = logger;
        // Build Kernel based on config
        var builder = Kernel.CreateBuilder();
        if (logger != null)
        {
            // Ideally we would pass ILoggerFactory to properly hook up SK logging, 
            // but for now we just use the passed logger for AgentService traces.
            // To enable SK logs, we would need to configure builder.Services.AddLogging(...) here.
        }
        
        var aiSection = config.GetSection("AI:TextGeneration");
        var provider = aiSection["Provider"] ?? "Ollama";
        var modelId = aiSection["ModelId"] ?? "llama3.1";
        var endpointUrl = aiSection["Endpoint"] ?? "http://localhost:11434";
        var apiKey = aiSection["ApiKey"];

        // If no key in config, try env var. If still null...
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        // For Ollama/LiteLLM with no auth, we need a non-empty string often
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = "dummy-key"; 
        }

        Spectre.Console.AnsiConsole.MarkupLine($"[grey]Configured AI: Provider={provider}, Model={modelId}, Endpoint={endpointUrl}[/]");
        _logger?.LogDebug("Configured AI: Provider={Provider}, Model={ModelId}, Endpoint={Endpoint}", provider, modelId, endpointUrl);

        var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };

        // Use OpenAI connector for any compatible provider (Ollama, LiteLLM, vLLM, etc.)
        builder.AddOpenAIChatCompletion(
            modelId: modelId,
            apiKey: apiKey, 
            endpoint: new Uri(endpointUrl),
            httpClient: httpClient);

        _kernel = builder.Build();
        _chat = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> ProcessQueryAsync(McpClient mcpClient, string userQuery)
    {
        _logger?.LogDebug("Processing query: {Query}", userQuery);
        // 1. Ask Server for relevant context (RAG)
        // Call "Query" tool
        var mcpResponse = await mcpClient.CallToolAsync("Query", new { query = userQuery, limit = 3 });
        
        string context = "";
        if (mcpResponse?.Result != null)
        {
             // Parse result. Result is { content: [ { type: "text", text: "JSON_STRING" } ] }
             var resultJson = JsonSerializer.Serialize(mcpResponse.Result);
             using var doc = JsonDocument.Parse(resultJson);
             if (doc.RootElement.TryGetProperty("content", out var contentArr) && contentArr.GetArrayLength() > 0)
             {
                 var text = contentArr[0].GetProperty("text").GetString();
                 // The text is a JSON string of SearchResultItems.
                 // We can just use it as raw context, or parse it to format better.
                 context = text ?? "";
                 _logger?.LogDebug("Retrieved context length: {Length}", context.Length);
                 _logger?.LogTrace("Retrieved Context: {Context}", context);
             }
        }

        if (string.IsNullOrWhiteSpace(context) || context == "[]")
        {
            _logger?.LogWarning("No context found for query.");
            return "I cannot answer this question because no relevant information was found in the knowledge base.";
        }

        // 2. Augment Prompt
        var prompt = $"""
            You are a helpful assistant. Use the following context to answer the user's question.
            
            CONTEXT:
            {context}
            
            QUESTION:
            {userQuery}
            
            ANSWER:
            """;

        _logger?.LogDebug("Sending prompt to LLM...");
        // 3. LLM Generation
        var response = await _chat.GetChatMessageContentAsync(prompt);
        _logger?.LogDebug("Received response from LLM.");
        
        return response.Content ?? "";
    }
}
