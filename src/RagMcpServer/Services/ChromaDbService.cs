#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0001
namespace RagMcpServer.Services;

using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ChromaDbService
{
    private readonly IMemoryStore _memoryStore;
    private const string CollectionName = "rag-collection";

    public ChromaDbService(IConfiguration configuration)
    {
        var endpoint = configuration["ChromaDb:Endpoint"] ?? "http://localhost:8000";
        var httpClient = new HttpClient { BaseAddress = new Uri(endpoint) };
        _memoryStore = new ChromaMemoryStore(new Microsoft.SemanticKernel.Connectors.Chroma.ChromaClient(httpClient));
    }

    public async Task SaveChunksAsync(IEnumerable<(string text, ReadOnlyMemory<float> embedding)> chunks, CancellationToken cancellationToken = default)
    {
        await foreach (var collectionName in _memoryStore.GetCollectionsAsync(cancellationToken))
        {
            // TODO: Use the collection names as needed.
            // This loop materializes the asynchronous sequence instead of trying to await it directly.
        }

        // ... rest of the implementation
    }

    public async Task<IEnumerable<string>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 3, CancellationToken cancellationToken = default)
    {
        await foreach (var result in _memoryStore.GetNearestMatchesAsync(
            CollectionName,
            queryEmbedding,
            limit: limit,
            minRelevanceScore: 0.7,
            cancellationToken: cancellationToken))
        {
            // TODO: Process each (MemoryRecord, double) result as needed.
            // This loop enumerates the asynchronous sequence instead of trying to await it directly.
        }

        return new List<string>();
    }
}
