using Microsoft.Extensions.Configuration;
using RagMcpServer.Configuration;
using Xunit;
using System.Collections.Generic;

namespace RagMcpServer.UnitTests.Configuration;

public class AIConfigTests
{
    [Fact]
    public void Should_Bind_Configuration_Correctly()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"AI:TextGeneration:Provider", "OpenAI"},
            {"AI:TextGeneration:ModelId", "gpt-4"},
            {"AI:TextGeneration:Endpoint", "https://api.openai.com/v1"},
            {"AI:TextGeneration:ApiKey", "sk-test"},
            {"AI:TextEmbedding:Provider", "Ollama"},
            {"AI:TextEmbedding:ModelId", "nomic-embed-text"},
            {"AI:TextEmbedding:Endpoint", "http://localhost:11434"},
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        var aiConfig = new AIConfig();
        configuration.GetSection(AIConfig.SectionName).Bind(aiConfig);

        // Assert
        Assert.Equal("OpenAI", aiConfig.TextGeneration.Provider);
        Assert.Equal("gpt-4", aiConfig.TextGeneration.ModelId);
        Assert.Equal("https://api.openai.com/v1", aiConfig.TextGeneration.Endpoint);
        Assert.Equal("sk-test", aiConfig.TextGeneration.ApiKey);

        Assert.Equal("Ollama", aiConfig.TextEmbedding.Provider);
        Assert.Equal("nomic-embed-text", aiConfig.TextEmbedding.ModelId);
        Assert.Equal("http://localhost:11434", aiConfig.TextEmbedding.Endpoint);
    }
}
