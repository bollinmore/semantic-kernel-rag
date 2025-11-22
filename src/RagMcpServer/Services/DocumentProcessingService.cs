namespace RagMcpServer.Services;

using System.Text;

public class DocumentProcessingService
{
    public async IAsyncEnumerable<string> GetDocumentChunksAsync(string path)
    {
        if (File.Exists(path))
        {
            foreach (var chunk in GetFileChunks(path))
            {
                yield return chunk;
            }
        }
        else if (Directory.Exists(path))
        {
            foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
            {
                foreach (var chunk in GetFileChunks(file))
                {
                    yield return chunk;
                }
            }
        }
    }

    private IEnumerable<string> GetFileChunks(string filePath)
    {
        // Simple chunking for text files for now.
        if (Path.GetExtension(filePath).Equals(".txt", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase))
        {
            // A real implementation would be more sophisticated.
            // This is just a placeholder.
            var content = File.ReadAllText(filePath, Encoding.UTF8);
            yield return content;
        }
    }
}
