#pragma warning disable SKEXP0050
namespace RagMcpServer.Services;

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;
using RagMcpServer.Configuration;

public class DocumentProcessingService
{
    private readonly DocumentProcessingConfig _config;

    public DocumentProcessingService(IOptions<AIConfig> config)
    {
        _config = config.Value.DocumentProcessing;
    }

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
        // PDF and other file types would be handled here
    }
}