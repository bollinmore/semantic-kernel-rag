using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RagMcpClient.Mcp;

namespace RagMcpClient.Services;

public class AgentService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;

    public AgentService(IConfiguration config)
    {
        // Build Kernel based on config
        var builder = Kernel.CreateBuilder();
        
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
             }
        }

        if (string.IsNullOrWhiteSpace(context) || context == "[]")
        {
            context = "No relevant documents found.";
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

        // 3. LLM Generation
        var response = await _chat.GetChatMessageContentAsync(prompt);
        return response.Content ?? "";
    }
}
