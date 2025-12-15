namespace RagApiServer.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IVectorDbService
{
    bool Exists { get; }
    Task SaveChunksAsync(IEnumerable<(string text, ReadOnlyMemory<float> embedding, string filePath)> chunks, string collectionName, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResultItem>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, string collectionName, int limit = 3, CancellationToken cancellationToken = default);
    Task<int> GetDocumentCountAsync(string collectionName, CancellationToken cancellationToken = default);
}

public record SearchResultItem(string Text, string FilePath, double Score);