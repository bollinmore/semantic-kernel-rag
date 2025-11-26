namespace RagMcpServer.UnitTests;

using Moq;
using RagMcpServer.Services;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class QueryServiceTests
{
    private readonly Mock<IVectorDbService> _vectorDbServiceMock;
    private readonly Mock<ITextEmbeddingGenerationService> _embeddingServiceMock;
    private readonly QueryService _service;

    public QueryServiceTests()
    {
        _vectorDbServiceMock = new Mock<IVectorDbService>();
        _vectorDbServiceMock.Setup(x => x.Exists).Returns(true);
        _embeddingServiceMock = new Mock<ITextEmbeddingGenerationService>();
        
        _service = new QueryService(_vectorDbServiceMock.Object, _embeddingServiceMock.Object);
    }

    [Fact]
    public async Task SearchAsync_GeneratesEmbeddingsAndSearchesDb()
    {
        // Arrange
        var query = "test query";
        var embedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
        var expectedResults = new List<SearchResultItem> 
        { 
            new SearchResultItem("Result 1", "file1.txt", 0.9),
            new SearchResultItem("Result 2", "file2.txt", 0.8)
        };
        
        _embeddingServiceMock.Setup(s => s.GenerateEmbeddingsAsync(
            It.Is<IList<string>>(l => l.Count == 1 && l[0] == query), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReadOnlyMemory<float>> { embedding });
        
        _vectorDbServiceMock.Setup(s => s.SearchAsync(embedding, 3, default))
            .ReturnsAsync(expectedResults);

        // Act
        var results = await _service.SearchAsync(query);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(2, results.Count());
        Assert.Equal("Result 1", results.First().Text);
        
        _embeddingServiceMock.Verify(s => s.GenerateEmbeddingsAsync(It.IsAny<IList<string>>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()), Times.Once);
        _vectorDbServiceMock.Verify(s => s.SearchAsync(embedding, 3, default), Times.Once);
    }
    
    [Fact]
    public async Task SearchAsync_PassesLimitCorrectly()
    {
        // Arrange
        var query = "test query";
        var limit = 5;
        var embedding = new ReadOnlyMemory<float>(new float[] { 1 });
        
        _embeddingServiceMock.Setup(s => s.GenerateEmbeddingsAsync(
            It.IsAny<IList<string>>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReadOnlyMemory<float>> { embedding });
            
        _vectorDbServiceMock.Setup(s => s.SearchAsync(embedding, limit, default))
            .ReturnsAsync(new List<SearchResultItem>());
            
        // Act
        await _service.SearchAsync(query, limit);
        
        // Assert
        _vectorDbServiceMock.Verify(s => s.SearchAsync(embedding, limit, default), Times.Once);
    }
}