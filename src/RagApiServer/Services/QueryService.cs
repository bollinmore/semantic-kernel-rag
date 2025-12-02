namespace RagApiServer.Services;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Embeddings;
using System.Collections.Generic;
using System.Threading; // Added for CancellationToken

public class QueryService
{
    private readonly IVectorDbService _vectorDbService;
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public QueryService(IVectorDbService vectorDbService, ITextEmbeddingGenerationService embeddingService)
    {
        _vectorDbService = vectorDbService;
        _embeddingService = embeddingService;
    }

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(string query, string collectionName, int limit = 3, CancellationToken cancellationToken = default)
    {
        // 1. Get embedding for the query
        var queryEmbedding = (await _embeddingService.GenerateEmbeddingsAsync(new[] { query }, cancellationToken: cancellationToken)).First();

        // 2. Search Vector DB
        return await _vectorDbService.SearchAsync(queryEmbedding, collectionName, limit, cancellationToken);
    }
}