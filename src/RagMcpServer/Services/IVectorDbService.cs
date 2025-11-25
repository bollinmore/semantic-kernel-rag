namespace RagMcpServer.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IVectorDbService
{
    Task SaveChunksAsync(IEnumerable<(string text, ReadOnlyMemory<float> embedding, string filePath)> chunks, CancellationToken cancellationToken = default);
    Task<IEnumerable<SearchResultItem>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 3, CancellationToken cancellationToken = default);
    Task<int> GetDocumentCountAsync(CancellationToken cancellationToken = default);
}

public record SearchResultItem(string Text, string FilePath, double Score);
