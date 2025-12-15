using System.Text.Json.Serialization;

namespace RagApiServer.Models;

public class ResultItem
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public Metadata Metadata { get; set; } = new();
}