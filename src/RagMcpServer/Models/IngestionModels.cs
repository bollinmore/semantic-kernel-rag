namespace RagMcpServer.Models;

public class IngestionRequest
{
    public string Path { get; set; } = string.Empty;
}

public class IngestionResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
