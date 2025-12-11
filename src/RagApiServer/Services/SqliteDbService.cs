#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0001
namespace RagApiServer.Services;

using Microsoft.SemanticKernel.Connectors.Sqlite;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Data.Sqlite; // For raw access

public class SqliteDbService : IVectorDbService
{
    private IMemoryStore? _memoryStore;
    private readonly string _connectionString;
    private readonly ILogger<SqliteDbService> _logger; 
    private string _absoluteDbPath = string.Empty; 

    public SqliteDbService(IConfiguration configuration, ILogger<SqliteDbService> logger) 
    {
        _logger = logger; 
        
        string? vectorDbPath = configuration["VectorDbPath"];
        if (!string.IsNullOrEmpty(vectorDbPath))
        {
             _logger.LogInformation("Using vector DB path from command line: {Path}", vectorDbPath);
             _connectionString = $"Data Source={vectorDbPath};Mode=ReadWriteCreate;Cache=Shared";
        }
        else
        {
            _connectionString = configuration.GetConnectionString("Sqlite") ?? "Data Source=rag.db";
        }
        
        _logger.LogInformation("Environment.CurrentDirectory: {CWD}", Environment.CurrentDirectory);
        _logger.LogInformation("AppContext.BaseDirectory: {BaseDir}", AppContext.BaseDirectory);

        try 
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder();
            builder.ConnectionString = _connectionString;
            if (builder.TryGetValue("Data Source", out var ds) || builder.TryGetValue("DataSource", out ds))
            {
                var path = ds as string;
                if (!string.IsNullOrEmpty(path))
                {
                    _absoluteDbPath = Path.GetFullPath(path);
                    
                    // Update the builder with the absolute path and then update _connectionString
                    builder["Data Source"] = _absoluteDbPath;
                    _connectionString = builder.ConnectionString;

                    _logger.LogInformation("SqliteDbService initialized. Resolved DB Path: {FullPath} (Exists: {Exists})", 
                        _absoluteDbPath, File.Exists(_absoluteDbPath));
                }
                else
                {
                    _logger.LogWarning("Connection string 'Data Source' value is empty. Using provided connection string as is: {ConnectionString}", _connectionString);
                }
            }
            else
            {
                _logger.LogWarning("Connection string does not contain 'Data Source' key. Using provided connection string as is: {ConnectionString}", _connectionString);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve absolute DB path from connection string: {ConnectionString}", _connectionString);
        }
        _logger.LogInformation("Final Connection String used: {ConnectionString}", _connectionString);
        
        // Debug: Run raw SQL check
        CheckRawDatabaseAccess();
    }

    private void CheckRawDatabaseAccess()
    {
        if (!Exists)
        {
            _logger.LogWarning("[Raw Check] Database file does not exist at {Path}. Skipping raw check.", _absoluteDbPath);
            return;
        }

        try 
        {
            // Handle relative paths in connection string for raw connection
            var connStr = _connectionString;
            // Microsoft.Data.Sqlite handles relative paths relative to CWD usually.
            
            using var connection = new SqliteConnection(connStr);
            connection.Open();
            
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='SKMemoryTable';";
            var tableName = cmd.ExecuteScalar() as string;
            
            if (string.IsNullOrEmpty(tableName))
            {
                _logger.LogWarning("[Raw Check] SKMemoryTable NOT FOUND in the database.");
            }
            else
            {
                _logger.LogInformation("[Raw Check] SKMemoryTable found.");
                
                var countCmd = connection.CreateCommand();
                countCmd.CommandText = "SELECT count(*) FROM SKMemoryTable;";
                var count = Convert.ToInt32(countCmd.ExecuteScalar());
                _logger.LogInformation("[Raw Check] SKMemoryTable contains {Count} rows.", count);
                
                var colCmd = connection.CreateCommand();
                colCmd.CommandText = "SELECT DISTINCT collection FROM SKMemoryTable;";
                using var reader = colCmd.ExecuteReader();
                var collections = new List<string>();
                while (reader.Read())
                {
                    collections.Add(reader.GetString(0));
                }
                _logger.LogInformation("[Raw Check] Distinct collections: {Collections}", string.Join(", ", collections));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Raw Check] Failed to inspect database directly.");
        }
    }

    public bool Exists => File.Exists(_absoluteDbPath);

    private async Task<IMemoryStore> GetStoreAsync(CancellationToken cancellationToken)
    {
        if (_memoryStore == null)
        {
            try
            {
                _logger.LogDebug("Attempting to connect to SqliteMemoryStore with path: {DbPath}", _absoluteDbPath);
                _memoryStore = await SqliteMemoryStore.ConnectAsync(_absoluteDbPath, cancellationToken);
                _logger.LogDebug("Connected to SqliteMemoryStore.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to SqliteMemoryStore.");
                throw; 
            }
        }
        return _memoryStore;
    }

    public async Task SaveChunksAsync(IEnumerable<(string text, ReadOnlyMemory<float> embedding, string filePath)> chunks, string collectionName, CancellationToken cancellationToken = default)
    {
        var store = await GetStoreAsync(cancellationToken);
        if (!await store.DoesCollectionExistAsync(collectionName, cancellationToken))
        {
            await store.CreateCollectionAsync(collectionName, cancellationToken);
        }

        foreach (var (text, embedding, filePath) in chunks)
        {
             var id = Guid.NewGuid().ToString();
             var memoryRecord = MemoryRecord.LocalRecord(
                 id: id,
                 text: text,
                 description: filePath, 
                 embedding: embedding);
             await store.UpsertAsync(collectionName, memoryRecord, cancellationToken);
        }
    }

    public async Task<IEnumerable<SearchResultItem>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, string collectionName, int limit = 3, CancellationToken cancellationToken = default)
    {
        if (!Exists)
        {
            _logger.LogWarning("[SK] Database file not found at {Path}. Returning empty results.", _absoluteDbPath);
            return Enumerable.Empty<SearchResultItem>();
        }

        var store = await GetStoreAsync(cancellationToken);
        
        // Debug logs again
        var collections = store.GetCollectionsAsync(cancellationToken);
        var collectionList = new List<string>();
        await foreach(var col in collections) collectionList.Add(col);
        _logger.LogInformation("[SK] Available collections: {Collections}", string.Join(", ", collectionList));

        if (!await store.DoesCollectionExistAsync(collectionName, cancellationToken))
        {
            _logger.LogWarning("[SK] Collection '{CollectionName}' not found.", collectionName);
            return Enumerable.Empty<SearchResultItem>();
        }

        var results = store.GetNearestMatchesAsync(
            collectionName,
            queryEmbedding,
            limit: limit,
            minRelevanceScore: 0.0, 
            cancellationToken: cancellationToken);

        var items = new List<SearchResultItem>();
        await foreach (var (record, score) in results)
        {
            items.Add(new SearchResultItem(record.Metadata.Text, record.Metadata.Description, score));
        }
        _logger.LogInformation("[SK] Found {Count} matches.", items.Count);
        return items;
    }

    public async Task<int> GetDocumentCountAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        // ...
        return 0;
    }
}
