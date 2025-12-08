namespace RagMcpServer.UnitTests;

using RagMcpServer.Services;
using RagMcpServer.Configuration;
using Xunit;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public class DocumentProcessingServiceTests
{
    private readonly Mock<ITextEmbeddingGenerationService> _embeddingServiceMock;
    private readonly Mock<IVectorDbService> _vectorDbServiceMock;
    private readonly DocumentProcessingService _service;
    private readonly IOptions<AIConfig> _mockAiConfigOptions;

    public DocumentProcessingServiceTests()
    {
        _mockAiConfigOptions = Options.Create(new AIConfig 
        { 
            DocumentProcessing = new DocumentProcessingConfig 
            { 
                MaxTokensPerLine = 100, 
                MaxTokensPerParagraph = 200, 
                OverlapTokens = 20 
            } 
        });

        _embeddingServiceMock = new Mock<ITextEmbeddingGenerationService>();
        _vectorDbServiceMock = new Mock<IVectorDbService>();
        _vectorDbServiceMock.Setup(x => x.Exists).Returns(true);

        _service = new DocumentProcessingService(_mockAiConfigOptions, _embeddingServiceMock.Object, _vectorDbServiceMock.Object, NullLogger<DocumentProcessingService>.Instance);
    }

    private async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    [Fact]
    public async Task GetDocumentChunksAsync_WithSingleFile_ReturnsCorrectChunks()
    {
        // Arrange
        var tempFile = Path.GetTempFileName() + ".txt";
        await File.WriteAllTextAsync(tempFile, "This is the first line.\nThis is the second line.");
        
        // Act
        var chunks = await ToListAsync(_service.GetDocumentChunksAsync(tempFile));

        // Assert
        Assert.NotEmpty(chunks);
        Assert.Contains(chunks, c => c.Content.Contains("This is the first line."));
        Assert.All(chunks, c => Assert.Equal(tempFile, c.FilePath));
        
        // Clean up
        File.Delete(tempFile);
    }

    [Fact]
    public async Task GetDocumentChunksAsync_WithDirectory_ReturnsChunksFromAllSupportedFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        var file1 = Path.Combine(tempDir, "test1.txt");
        var file2 = Path.Combine(tempDir, "test2.md");
        await File.WriteAllTextAsync(file1, "Content from file one.");
        await File.WriteAllTextAsync(file2, "Content from file two.");
        await File.WriteAllTextAsync(Path.Combine(tempDir, "test3.unsupported"), "This should be ignored.");

        // Act
        var chunks = await ToListAsync(_service.GetDocumentChunksAsync(tempDir));

        // Assert
        Assert.Equal(2, chunks.Count);
        Assert.Contains(chunks, c => c.Content.Contains("Content from file one.") && c.FilePath == file1);
        Assert.Contains(chunks, c => c.Content.Contains("Content from file two.") && c.FilePath == file2);

        // Clean up
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task GetDocumentChunksAsync_WithLargeFile_SplitsIntoSmallerChunks()
    {
        // Arrange
        var tempFile = Path.GetTempFileName() + ".txt";
        // Create a long line with spaces to allow splitting
        var longLine = string.Join(" ", Enumerable.Repeat("word", 100)); // 100 words * 5 chars = 500 chars approx
        var content = $"{longLine} {longLine}"; // ~1000 chars
        await File.WriteAllTextAsync(tempFile, content);

        // Act
        var chunks = await ToListAsync(_service.GetDocumentChunksAsync(tempFile));

        // Assert
        Assert.True(chunks.Count > 1, "The content should have been split into multiple chunks.");
        // We check the Content length of the tuple. 
        // MaxTokensPerParagraph is 200. Avg token is 4 chars. So chunk should be ~800 chars max.
        // Let's just assert it's less than the total length.
        Assert.All(chunks, chunk => Assert.True(chunk.Content.Length < content.Length));

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
        var chunks = await ToListAsync(_service.GetDocumentChunksAsync(tempDir));

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
        var chunks = await ToListAsync(_service.GetDocumentChunksAsync(nonExistentPath));

        // Assert
        Assert.Empty(chunks);
    }
}
