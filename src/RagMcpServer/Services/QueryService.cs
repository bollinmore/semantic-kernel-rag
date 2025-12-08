namespace RagMcpServer.Services;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Embeddings;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

public class QueryService
{
    private readonly IVectorDbService _vectorDbService;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly ILogger<QueryService> _logger;

    public QueryService(IVectorDbService vectorDbService, ITextEmbeddingGenerationService embeddingService, ILogger<QueryService> logger)
    {
        _vectorDbService = vectorDbService;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(string query, int limit = 3)
    {
        _logger.LogDebug("Searching for: {Query} (Limit: {Limit})", query, limit);
        
        // 1. Get embedding for the query
        _logger.LogDebug("Generating query embedding...");
        var queryEmbedding = (await _embeddingService.GenerateEmbeddingsAsync(new[] { query })).First();

        // 2. Search Vector DB
        return await _vectorDbService.SearchAsync(queryEmbedding, limit);
    }
}