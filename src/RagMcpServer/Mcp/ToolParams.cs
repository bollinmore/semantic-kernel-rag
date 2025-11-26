using System.Text.Json;
using System.Text.Json.Serialization;

namespace RagMcpServer.Mcp;

public class McpToolCallParams
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public JsonElement Arguments { get; set; }
}
