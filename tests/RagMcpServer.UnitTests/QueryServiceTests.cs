namespace RagMcpServer.UnitTests;

using Moq;
using RagMcpServer.Services;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Collections.Generic;

public class QueryServiceTests
{
    private readonly Mock<IVectorDbService> _vectorDbServiceMock;
    private readonly Mock<OllamaEmbeddingService> _embeddingServiceMock;
    private readonly Mock<Kernel> _kernelMock;
    private readonly QueryService _service;

    public QueryServiceTests()
    {
        // Mock IConfiguration for services if needed
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        
        _vectorDbServiceMock = new Mock<IVectorDbService>();
        _embeddingServiceMock = new Mock<OllamaEmbeddingService>(configMock.Object, "nomic-embed-text");
        _kernelMock = new Mock<Kernel>();
        
        _service = new QueryService(_vectorDbServiceMock.Object, _embeddingServiceMock.Object, _kernelMock.Object);
    }

    [Fact]
    public async Task QueryAsync_WhenNoDocumentsFound_ReturnsInformativeMessage()
    {
        // Arrange
        var query = "test query";
        var embedding = new ReadOnlyMemory<float>(new float[] { 1, 2, 3 });
        
        _embeddingServiceMock.Setup(s => s.GenerateEmbeddingsAsync(It.IsAny<IList<string>>(), null, default))
            .ReturnsAsync(new List<ReadOnlyMemory<float>> { embedding });
        
        _vectorDbServiceMock.Setup(s => s.SearchAsync(embedding, 3, default))
            .ReturnsAsync(new List<string>());

        // Act
        var response = await _service.QueryAsync(query);

        // Assert
        Assert.Equal("No relevant information found in the documents.", response.Answer);
        Assert.Empty(response.SourceDocuments);
    }

    [Fact]
    public async Task QueryAsync_WhenDocumentsFound_InvokesKernelAndReturnsAnswer()
    {
        // Arrange
        var query = "What is the capital of France?";
        var embedding = new ReadOnlyMemory<float>(new float[] { 1, 2, 3 });
        var searchResults = new List<string> { "Paris is the capital of France." };
        var expectedAnswer = "The capital of France is Paris.";

        _embeddingServiceMock.Setup(s => s.GenerateEmbeddingsAsync(It.IsAny<IList<string>>(), null, default))
            .ReturnsAsync(new List<ReadOnlyMemory<float>> { embedding });

        _vectorDbServiceMock.Setup(s => s.SearchAsync(embedding, 3, default))
            .ReturnsAsync(searchResults);
        
        var kernelFunctionMock = new Mock<KernelFunction>();
        kernelFunctionMock.SetupGet(f => f.Name).Returns("test");
        var functionResult = new FunctionResult(kernelFunctionMock.Object, expectedAnswer);

        _kernelMock.Setup(k => k.InvokePromptAsync(It.IsAny<string>(), It.IsAny<KernelArguments>(), default))
            .ReturnsAsync(functionResult);

        // Act
        var response = await _service.QueryAsync(query);

        // Assert
        Assert.Equal(expectedAnswer, response.Answer);
        Assert.Single(response.SourceDocuments);
        Assert.Equal(searchResults.First(), response.SourceDocuments.First().Content);
        _kernelMock.Verify(k => k.InvokePromptAsync(It.Is<string>(s => s.Contains(searchResults.First())), It.IsAny<KernelArguments>(), default), Times.Once);
    }
}
