namespace RagMcpServer.UnitTests;

using RagMcpServer.Services;
using Xunit;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class DocumentProcessingServiceTests
{
    private readonly DocumentProcessingService _service = new();

    [Fact]
    public async Task GetDocumentChunksAsync_WithSingleFile_ReturnsCorrectChunks()
    {
        // Arrange
        var tempFile = Path.GetTempFileName() + ".txt";
        await File.WriteAllTextAsync(tempFile, "This is the first line.\nThis is the second line.");
        
        // Act
        var chunks = await _service.GetDocumentChunksAsync(tempFile).ToListAsync();

        // Assert
        Assert.NotEmpty(chunks);
        Assert.Contains("This is the first line.", chunks.First());
        
        // Clean up
        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDocumentChunksAsync_WithDirectory_ReturnsChunksFromAllSupportedFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        await File.WriteAllTextAsync(Path.Combine(tempDir, "test1.txt"), "Content from file one.");
        await File.WriteAllTextAsync(Path.Combine(tempDir, "test2.md"), "Content from file two.");
        await File.WriteAllTextAsync(Path.Combine(tempDir, "test3.unsupported"), "This should be ignored.");

        // Act
        var chunks = await _service.GetDocumentChunksAsync(tempDir).ToListAsync();

        // Assert
        Assert.Equal(2, chunks.Count);
        Assert.Contains("Content from file one.", chunks);
        Assert.Contains("Content from file two.", chunks);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetDocumentChunksAsync_WithLargeFile_SplitsIntoSmallerChunks()
    {
        // Arrange
        var tempFile = Path.GetTempFileName() + ".txt";
        var longLine = new string('a', 600);
        var content = $"{longLine}\n{longLine}";
        await File.WriteAllTextAsync(tempFile, content);

        // Act
        var chunks = await _service.GetDocumentChunksAsync(tempFile).ToListAsync();

        // Assert
        Assert.True(chunks.Count > 1, "The content should have been split into multiple chunks.");
        Assert.All(chunks, chunk => Assert.True(chunk.Length <= 512));

        // Clean up
        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDocumentChunksAsync_WithEmptyDirectory_ReturnsNoChunks()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        // Act
        var chunks = await _service.GetDocumentChunksAsync(tempDir).ToListAsync();

        // Assert
        Assert.Empty(chunks);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetDocumentChunksAsync_WithNonExistentPath_ReturnsNoChunks()
    {
        // Arrange
        var nonExistentPath = "/non/existent/path/that/does/not/exist";

        // Act
        var chunks = await _service.GetDocumentChunksAsync(nonExistentPath).ToListAsync();

        // Assert
        Assert.Empty(chunks);
    }
}