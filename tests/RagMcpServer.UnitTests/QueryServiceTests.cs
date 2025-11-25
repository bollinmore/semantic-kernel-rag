namespace RagMcpServer.UnitTests;

using Moq;
using RagMcpServer.Services;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

public class QueryServiceTests
{
    private readonly Mock<IVectorDbService> _vectorDbServiceMock;
    private readonly Mock<ITextEmbeddingGenerationService> _embeddingServiceMock;
    private readonly Mock<IChatCompletionService> _chatCompletionServiceMock;
    private readonly Kernel _kernel;
    private readonly QueryService _service;

    public QueryServiceTests()
    {
        _vectorDbServiceMock = new Mock<IVectorDbService>();
        _embeddingServiceMock = new Mock<ITextEmbeddingGenerationService>();
        _chatCompletionServiceMock = new Mock<IChatCompletionService>();

        // Create a real Kernel with mocked services
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(_chatCompletionServiceMock.Object);
        _kernel = builder.Build();
        
        _service = new QueryService(_vectorDbServiceMock.Object, _embeddingServiceMock.Object, _kernel);
    }

    [Fact]
    public async Task QueryAsync_WhenNoDocumentsFound_ReturnsInformativeMessage()
    {
        // Arrange
        var query = "test query";
        var embedding = new ReadOnlyMemory<float>(new float[] { 1, 2, 3 });
        
        _embeddingServiceMock.Setup(s => s.GenerateEmbeddingsAsync(
            It.IsAny<IList<string>>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReadOnlyMemory<float>> { embedding });
        
        _vectorDbServiceMock.Setup(s => s.SearchAsync(embedding, 3, default))
            .ReturnsAsync(new List<SearchResultItem>());

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
        var searchResults = new List<SearchResultItem> 
        { 
            new SearchResultItem("Paris is the capital of France.", "test.txt", 0.9) 
        };
        var expectedAnswer = "The capital of France is Paris.";

        _embeddingServiceMock.Setup(s => s.GenerateEmbeddingsAsync(
            It.IsAny<IList<string>>(), 
            It.IsAny<Kernel>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReadOnlyMemory<float>> { embedding });

        _vectorDbServiceMock.Setup(s => s.SearchAsync(embedding, 3, default))
            .ReturnsAsync(searchResults);
        
        // Mock the chat completion call that Kernel.InvokePromptAsync eventually makes
        // We need to mock the actual interface method: GetChatMessageContentsAsync (plural)
        _chatCompletionServiceMock.Setup(c => c.GetChatMessageContentsAsync(
            It.IsAny<ChatHistory>(),
            It.IsAny<PromptExecutionSettings>(),
            It.IsAny<Kernel>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { new ChatMessageContent(AuthorRole.Assistant, expectedAnswer) });

        // Act
        var response = await _service.QueryAsync(query, includeSources: true); // Include sources to verify them

        // Assert
        Assert.Equal(expectedAnswer, response.Answer);
        Assert.Single(response.SourceDocuments);
        Assert.Equal(searchResults.First().Text, response.SourceDocuments.First().Content);
    }
}
