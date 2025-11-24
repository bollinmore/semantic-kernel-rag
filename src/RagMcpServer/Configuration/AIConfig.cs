using System.ComponentModel.DataAnnotations;

namespace RagMcpServer.Configuration;

public class AIConfig
{
    public const string SectionName = "AI";

    public AIServiceConfig TextGeneration { get; set; } = new();
    public AIServiceConfig TextEmbedding { get; set; } = new();
}

public class AIServiceConfig
{
    public string Provider { get; set; } = "Ollama"; // Default to Ollama
    public string ModelId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
