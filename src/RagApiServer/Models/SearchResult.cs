using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RagApiServer.Models;

public class SearchResult
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("results")]
    public List<ResultItem> Results { get; set; } = new();
}