namespace RagMcpServer.Models;

public class QueryRequest
{
    public string Query { get; set; } = string.Empty;
}

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceDocument> SourceDocuments { get; set; } = new();
}

public class SourceDocument
{
    public string SourcePath { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
