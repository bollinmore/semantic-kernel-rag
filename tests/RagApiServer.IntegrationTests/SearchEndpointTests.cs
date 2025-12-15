using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using RagApiServer;
using RagApiServer.Models;
using RagApiServer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;

namespace RagApiServer.IntegrationTests;

public class SearchEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SearchEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Search_ValidRequest_Returns200AndResults()
    {
        // Arrange
        var mockDb = new Mock<IVectorDbService>();
        mockDb.Setup(db => db.SearchAsync(It.IsAny<ReadOnlyMemory<float>>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync(new List<SearchResultItem> 
              { 
                  new SearchResultItem("Content1", "Source1.txt", 0.9),
                  new SearchResultItem("Content2", "Source2.txt", 0.8)
              });

        var mockEmbedding = new Mock<ITextEmbeddingGenerationService>();
        mockEmbedding.Setup(e => e.GenerateEmbeddingsAsync(It.IsAny<IList<string>>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<ReadOnlyMemory<float>> { new float[] { 0.1f, 0.2f } });

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IVectorDbService>();
                services.AddSingleton(mockDb.Object);

                services.RemoveAll<ITextEmbeddingGenerationService>();
                services.AddSingleton(mockEmbedding.Object);
            });
        }).CreateClient();

        var request = new SearchRequest 
        { 
            CollectionName = "default",
            SourceType = "text",
            Query = "test", 
            TopK = 2,
            TopN = 5,
            ScoreThreshold = 0.5f
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/rag/search", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SearchResult>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Results.Count);
        Assert.Equal("Content1", result.Results[0].Content);
        Assert.Equal("Source1.txt", result.Results[0].Metadata.Source);
    }

    [Fact]
    public async Task Search_InvalidTopK_Returns400()
    {
        var client = _factory.CreateClient();

        var request = new SearchRequest 
        { 
            CollectionName = "default",
            SourceType = "text",
            Query = "test", 
            TopK = 101 // Invalid
        }; 

        var response = await client.PostAsJsonAsync("/api/rag/search", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
