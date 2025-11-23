namespace RagMcpServer.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IVectorDbService
{
    Task SaveChunksAsync(IEnumerable<(string text, ReadOnlyMemory<float> embedding)> chunks, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> SearchAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 3, CancellationToken cancellationToken = default);
}
