#pragma warning disable SKEXP0050
namespace RagMcpServer.Services;

using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.SemanticKernel.Text;

public class DocumentProcessingService
{
    public async IAsyncEnumerable<string> GetDocumentChunksAsync(string path)
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

    private async IAsyncEnumerable<string> GetFileChunks(string filePath)
    {
        if (Path.GetExtension(filePath).Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var lines = TextChunker.SplitPlainTextLines(content, 128);
            var chunks = TextChunker.SplitPlainTextParagraphs(lines, 512);

            foreach (var chunk in chunks)
            {
                yield return chunk;
            }
        }
        // PDF and other file types would be handled here
    }
}