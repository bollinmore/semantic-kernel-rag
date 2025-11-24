using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RagMcpServer.Configuration;
using RagMcpServer.Extensions;
using RagMcpServer.Services;
using Xunit;
using System.Collections.Generic;

namespace RagMcpServer.UnitTests.Services;

public class EmbeddingRegistrationTests
{
    [Fact]
    public void AddAIServices_WhenProviderIsOllama_RegistersOllamaEmbeddingService()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            {"AI:TextEmbedding:Provider", "Ollama"},
            {"AI:TextEmbedding:Endpoint", "http://localhost:11434"},
            {"AI:TextEmbedding:ModelId", "nomic-embed-text"}
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
            
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<AIConfig>(configuration.GetSection(AIConfig.SectionName));
        
        // Act
        services.AddAIServices();
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
        Assert.IsType<OllamaEmbeddingService>(embeddingService);
    }

    [Fact]
    public void AddAIServices_WhenProviderIsOpenAI_RegistersOpenAIEmbeddingService()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            {"AI:TextEmbedding:Provider", "OpenAI"},
            {"AI:TextEmbedding:Endpoint", "http://localhost:4000"},
            {"AI:TextEmbedding:ModelId", "text-embedding-ada-002"},
            {"AI:TextEmbedding:ApiKey", "sk-test"}
        };
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
            
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<AIConfig>(configuration.GetSection(AIConfig.SectionName));
        
        // Act
        services.AddAIServices();
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
        Assert.IsType<OpenAITextEmbeddingGenerationService>(embeddingService);
    }
}
