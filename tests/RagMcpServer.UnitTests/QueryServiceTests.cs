namespace RagMcpServer.UnitTests;

using System.Threading.Tasks;
using Moq;
using RagMcpServer.Services;
using Xunit;

public class QueryServiceTests
{
    [Fact]
    public async Task QueryAsync_ReturnsPlaceholderAnswer()
    {
        // Arrange
        var chromaDbServiceMock = new Mock<ChromaDbService>(null);
        var ollamaEmbeddingServiceMock = new Mock<OllamaEmbeddingService>(null);
        var service = new QueryService(chromaDbServiceMock.Object, ollamaEmbeddingServiceMock.Object);

        // Act
        var response = await service.QueryAsync("test query");

        // Assert
        Assert.Equal("This is a placeholder answer.", response.Answer);
    }
}
