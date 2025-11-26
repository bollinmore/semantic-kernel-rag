#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0001
namespace RagMcpServer.Services;

using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; // Added for logging
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
    private readonly ILogger<SqliteDbService> _logger; // Added logger

    public SqliteDbService(IConfiguration configuration, ILogger<SqliteDbService> logger) // Modified constructor
    {
        _logger = logger; // Assign logger
        _connectionString = configuration.GetConnectionString("Sqlite") ?? "Data Source=rag.db";
        _logger.LogInformation("SqliteDbService initialized with connection string: {ConnectionString}", _connectionString); // Log connection string
    }

    public bool Exists
    {
        get
        {
            try 
            {
                var path = _connectionString;
                // Simple parsing for "Data Source=" or "DataSource="
                var builder = new System.Data.Common.DbConnectionStringBuilder();
                builder.ConnectionString = _connectionString;
                
                object? file = null;
                if (builder.TryGetValue("Data Source", out var f1))
                {
                    file = f1;
                }
                else if (builder.TryGetValue("DataSource", out var f2))
                {
                    file = f2;
                }
                
                if (file != null)
                {
                    path = (string)file;
                }
                
                // If path is relative, it depends on working dir, but let's check it.
                // Assuming local file.
                return System.IO.File.Exists(path);
            }
            catch
            {
                // Fallback: check if the string itself is a file that exists
                return System.IO.File.Exists(_connectionString);
            }
        }
    }

    private async Task<IMemoryStore> GetStoreAsync(CancellationToken cancellationToken)
    {
        if (_memoryStore == null)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to SqliteMemoryStore using: {ConnectionString}", _connectionString);
                _memoryStore = await SqliteMemoryStore.ConnectAsync(_connectionString, cancellationToken);
                _logger.LogInformation("Successfully connected to SqliteMemoryStore.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to SqliteMemoryStore using: {ConnectionString}", _connectionString);
                throw; // Re-throw to propagate the error
            }
        }
        return _memoryStore;
    }

    public async Task SaveChunksAsync(IEnumerable<(string text, ReadOnlyMemory<float> embedding, string filePath)> chunks, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        
        try
        {
            if (!await store.DoesCollectionExistAsync(CollectionName, cancellationToken))
            {
                _logger.LogInformation("Collection '{CollectionName}' does not exist. Creating it.", CollectionName);
                await store.CreateCollectionAsync(CollectionName, cancellationToken);
                _logger.LogInformation("Collection '{CollectionName}' created.", CollectionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or check existence of collection '{CollectionName}'.", CollectionName);
            throw;
        }

        foreach (var (text, embedding, filePath) in chunks)
        {
             var id = Guid.NewGuid().ToString();
             // We store the filePath in the Description field.
             var memoryRecord = MemoryRecord.LocalRecord(
                 id: id,
                 text: text,
                 description: filePath, 
                 embedding: embedding);
             
             try
             {
                 await store.UpsertAsync(CollectionName, memoryRecord, cancellationToken);
                 _logger.LogDebug("Saved chunk with ID '{Id}' from '{FilePath}' to collection '{CollectionName}'.", id, filePath, CollectionName);
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Failed to upsert memory record with ID '{Id}' from '{FilePath}' to collection '{CollectionName}'.", id, filePath, CollectionName);
                 throw;
             }
        }
        _logger.LogInformation("Finished saving {ChunkCount} chunks to collection '{CollectionName}'.", chunks.Count(), CollectionName);
    }

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 3, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        
        try
        {
            if (!await store.DoesCollectionExistAsync(CollectionName, cancellationToken))
            {
                _logger.LogWarning("Search attempted on non-existent collection '{CollectionName}'. Returning empty results.", CollectionName);
                return Enumerable.Empty<SearchResultItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check existence of collection '{CollectionName}' during search.", CollectionName);
            throw;
        }

        _logger.LogInformation("Searching collection '{CollectionName}' for nearest matches (limit: {Limit}).", CollectionName, limit);
        var items = new List<SearchResultItem>();
        try
        {
            var results = store.GetNearestMatchesAsync(
                CollectionName,
                queryEmbedding,
                limit: limit,
                minRelevanceScore: 0.3,
                cancellationToken: cancellationToken);

            await foreach (var (record, score) in results)
            {
                items.Add(new SearchResultItem(record.Metadata.Text, record.Metadata.Description, score));
            }
            _logger.LogInformation("Found {ResultCount} relevant items from collection '{CollectionName}'.", items.Count, CollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get nearest matches from collection '{CollectionName}'.", CollectionName);
            throw;
        }
        return items;
    }

    public async Task<int> GetDocumentCountAsync(CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        
        try 
        {
             var connStr = _connectionString;
             if (!connStr.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && 
                 !connStr.Contains("DataSource=", StringComparison.OrdinalIgnoreCase))
             {
                 connStr = $"Data Source={connStr}";
             }

             using var connection = new Microsoft.Data.Sqlite.SqliteConnection(connStr);
             await connection.OpenAsync(cancellationToken);
             
             // The table name created by SK's SqliteMemoryStore is typically "SKMemoryTable"
             // but we should verify if it depends on the collection name.
             // Looking at SK source, it uses a fixed table 'SKMemoryTable' and filters by 'collection' column.
             
             var command = connection.CreateCommand();
             command.CommandText = "SELECT COUNT(*) FROM SKMemoryTable WHERE collection = @collection";
             command.Parameters.AddWithValue("@collection", CollectionName);
             
             var result = await command.ExecuteScalarAsync(cancellationToken);
             return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document count for collection '{CollectionName}'.", CollectionName);
            // If table doesn't exist yet, return 0
            return 0;
        }
    }
}
