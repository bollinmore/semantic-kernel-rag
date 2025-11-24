using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RagMcpServer.Configuration;
using RagMcpServer.Extensions;
using RagMcpServer.Services;
using Xunit;
using System.Collections.Generic;

namespace RagMcpServer.UnitTests.Services;

public class ChatCompletionRegistrationTests
{
    [Fact]
    public void AddAIServices_WhenProviderIsOllama_RegistersOllamaService()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            {"AI:TextGeneration:Provider", "Ollama"},
            {"AI:TextGeneration:Endpoint", "http://localhost:11434"},
            {"AI:TextGeneration:ModelId", "llama3.1"}
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
        var chatService = serviceProvider.GetService<IChatCompletionService>();
        Assert.NotNull(chatService);
        Assert.IsType<OllamaChatCompletionService>(chatService);
    }

    [Fact]
    public void AddAIServices_WhenProviderIsOpenAI_RegistersOpenAIService()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            {"AI:TextGeneration:Provider", "OpenAI"},
            {"AI:TextGeneration:Endpoint", "http://localhost:4000"},
            {"AI:TextGeneration:ModelId", "gpt-4"},
            {"AI:TextGeneration:ApiKey", "sk-test"}
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
        var chatService = serviceProvider.GetService<IChatCompletionService>();
        Assert.NotNull(chatService);
        Assert.IsType<OpenAIChatCompletionService>(chatService);
    }
}
