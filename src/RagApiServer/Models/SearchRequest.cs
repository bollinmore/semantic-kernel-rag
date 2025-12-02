using System.ComponentModel;
using System.Text.Json.Serialization;

namespace RagApiServer.Models;

public class SearchRequest
{
    [JsonPropertyName("collection_name")]
    public required string CollectionName { get; set; }

    [JsonPropertyName("source_type")]
    public required string SourceType { get; set; }

    [JsonPropertyName("query")]
    public required string Query { get; set; }

    [JsonPropertyName("top_k")]
    [DefaultValue(50)]
    public int TopK { get; set; } = 50;

    [JsonPropertyName("top_n")]
    [DefaultValue(10)]
    public int TopN { get; set; } = 10;

    [JsonPropertyName("score_threshold")]
    [DefaultValue(0.6f)]
    public float ScoreThreshold { get; set; } = 0.6f;
}