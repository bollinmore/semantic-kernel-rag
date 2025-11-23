#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0001
namespace RagMcpServer.Services;

using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SqliteDbService : IVectorDbService
{
    private IMemoryStore? _memoryStore;
    private const string CollectionName = "rag-collection";
    private readonly string _connectionString;

    public SqliteDbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Sqlite") ?? "Data Source=rag.db";
    }

    private async Task<IMemoryStore> GetStoreAsync(CancellationToken cancellationToken)
    {
        if (_memoryStore == null)
        {
            _memoryStore = await SqliteMemoryStore.ConnectAsync(_connectionString, cancellationToken);
        }
        return _memoryStore;
    }

    public async Task SaveChunksAsync(IEnumerable<(string text, ReadOnlyMemory<float> embedding)> chunks, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        
        if (!await store.DoesCollectionExistAsync(CollectionName, cancellationToken))
        {
            await store.CreateCollectionAsync(CollectionName, cancellationToken);
        }

        foreach (var (text, embedding) in chunks)
        {
             var id = Guid.NewGuid().ToString();
             var memoryRecord = MemoryRecord.LocalRecord(
                 id: id,
                 text: text,
                 description: null,
                 embedding: embedding);
             
             await store.UpsertAsync(CollectionName, memoryRecord, cancellationToken);
        }
    }

    public async Task<IEnumerable<string>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 3, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        
        if (!await store.DoesCollectionExistAsync(CollectionName, cancellationToken))
        {
            return Enumerable.Empty<string>();
        }

        var results = store.GetNearestMatchesAsync(
            CollectionName,
            queryEmbedding,
            limit: limit,
            minRelevanceScore: 0.25,
            cancellationToken: cancellationToken);

        var resultStrings = new List<string>();
        await foreach (var (record, score) in results)
        {
            resultStrings.Add(record.Metadata.Text);
        }
        return resultStrings;
    }
}
