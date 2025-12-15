namespace RagApiServer.Models;

public class InfoResponse
{
    public VectorDbInfo VectorDb { get; set; } = new();
}

public class VectorDbInfo
{
    public string CollectionName { get; set; } = string.Empty;
    public int DocumentCount { get; set; }
    public string VectorDbType { get; set; } = "Sqlite"; // e.g., Sqlite
    public string Algorithm { get; set; } = "Cosine Similarity"; // e.g., Cosine Similarity
    public string EmbeddingModelName { get; set; } = string.Empty;
    public string EmbeddingProvider { get; set; } = string.Empty;
    public int EmbeddingDimensions { get; set; }
}
