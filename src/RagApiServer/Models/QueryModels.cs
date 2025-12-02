namespace RagApiServer.Models;

using System.Collections.Generic;

public class QueryRequest
{
    public string Query { get; set; } = string.Empty;
    public bool IncludeSources { get; set; } = false;
}

public class QueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<SourceDocument> SourceDocuments { get; set; } = new();
}

public class SourceDocument
{
    public string Content { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
}
