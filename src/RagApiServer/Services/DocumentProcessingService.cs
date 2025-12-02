#pragma warning disable SKEXP0050
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;
using Microsoft.SemanticKernel.Embeddings;
using RagApiServer.Configuration;
using System.Threading;

namespace RagApiServer.Services;

public class DocumentProcessingService
{
    private readonly DocumentProcessingConfig _config;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IVectorDbService _vectorDbService;

    public DocumentProcessingService(
        IOptions<AIConfig> config,
        ITextEmbeddingGenerationService embeddingService,
        IVectorDbService vectorDbService)
    {
        _config = config.Value.DocumentProcessing;
        _embeddingService = embeddingService;
        _vectorDbService = vectorDbService;
    }

    public async Task ProcessAndSaveAsync(string text, string filePath, string collectionName, CancellationToken cancellationToken = default)
    {
        // 1. Chunking
        var lines = TextChunker.SplitPlainTextLines(text, _config.MaxTokensPerLine);
        var chunks = TextChunker.SplitPlainTextParagraphs(lines, _config.MaxTokensPerParagraph, _config.OverlapTokens);
        
        if (chunks.Count == 0) return;

        // 2. Embedding
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunks, cancellationToken: cancellationToken);

        // 3. Zip and Save
        var records = new List<(string text, ReadOnlyMemory<float> embedding, string filePath)>();
        for (int i = 0; i < chunks.Count; i++)
        {
            records.Add((chunks[i], embeddings[i], filePath));
        }

        await _vectorDbService.SaveChunksAsync(records, collectionName, cancellationToken);
    }
    
    // Legacy methods kept if needed or can be removed if not used by anyone else
    public async IAsyncEnumerable<(string Content, string FilePath)> GetDocumentChunksAsync(string path)
    {
        if (File.Exists(path))
        {
            await foreach (var chunk in GetFileChunks(path))
            {
                yield return chunk;
            }
        }
        else if (Directory.Exists(path))
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                await foreach (var chunk in GetFileChunks(file))
                {
                    yield return chunk;
                }
            }
        }
    }

    private async IAsyncEnumerable<(string Content, string FilePath)> GetFileChunks(string filePath)
    {
        if (Path.GetExtension(filePath).Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var lines = TextChunker.SplitPlainTextLines(content, _config.MaxTokensPerLine);
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, _config.MaxTokensPerParagraph, _config.OverlapTokens);

            foreach (var chunk in chunks)
            {
                yield return (chunk, filePath);
            }
        }
    }
}