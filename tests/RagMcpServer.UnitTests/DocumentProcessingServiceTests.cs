namespace RagMcpServer.UnitTests;

using RagMcpServer.Services;
using Xunit;

public class DocumentProcessingServiceTests
{
    [Fact]
    public async Task GetDocumentChunksAsync_WithFile_ReturnsContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, "Hello, World!");
        var service = new DocumentProcessingService();

        // Act
        var chunks = new List<string>();
        await foreach (var chunk in service.GetDocumentChunksAsync(tempFile))
        {
            chunks.Add(chunk);
        }

        // Assert
        Assert.Single(chunks);
        Assert.Equal("Hello, World!", chunks[0]);

        // Clean up
        File.Delete(tempFile);
    }
}
