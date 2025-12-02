using System.Text.Json.Serialization;

namespace RagApiServer.Models;

public class Metadata
{
    [JsonPropertyName("page_number")]
    public string PageNumber { get; set; } = "0";

    [JsonPropertyName("chunk_index")]
    public string ChunkIndex { get; set; } = "0";

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}