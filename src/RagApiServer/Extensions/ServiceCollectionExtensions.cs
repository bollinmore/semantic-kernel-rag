using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Azure.AI.OpenAI;
using Azure;
using RagApiServer.Configuration;
using RagApiServer.Services;

namespace RagApiServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAIServices(this IServiceCollection services)
    {
        services.AddSingleton<IChatCompletionService>(sp => 
        {
            var config = sp.GetRequiredService<IOptions<AIConfig>>().Value.TextGeneration;
            
            if (string.IsNullOrWhiteSpace(config.Provider))
            {
                throw new InvalidOperationException("AI:TextGeneration:Provider configuration is missing.");
            }

            if (string.Equals(config.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(config.Provider, "LiteLLM", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(config.ModelId))
                {
                    throw new InvalidOperationException("AI:TextGeneration:ModelId is required for OpenAI provider.");
                }

                // ApiKey is required by OpenAIChatCompletionService constructor, pass empty string if missing but it might fail if real OpenAI
                var apiKey = !string.IsNullOrEmpty(config.ApiKey) ? config.ApiKey : "dummy-key"; 

                // For custom endpoints (LiteLLM), use OpenAIClient explicitly to ensure the endpoint is respected.
                // Note: This uses AzureKeyCredential which sends 'api-key' header. LiteLLM supports this.
                // It also uses Azure-style paths /openai/deployments/... which LiteLLM also typically supports.
                if (!string.IsNullOrEmpty(config.Endpoint))
                {
                    var client = new OpenAIClient(new Uri(config.Endpoint), new AzureKeyCredential(apiKey));
                    return new OpenAIChatCompletionService(config.ModelId, client);
                }

                return new OpenAIChatCompletionService(
                    modelId: config.ModelId,
                    apiKey: apiKey
                );
            }
            
            // Default to Ollama
            return new OllamaChatCompletionService(sp.GetRequiredService<IOptions<AIConfig>>());
        });

        services.AddSingleton<ITextEmbeddingGenerationService>(sp => 
        {
            var config = sp.GetRequiredService<IOptions<AIConfig>>().Value.TextEmbedding;
            
            if (string.IsNullOrWhiteSpace(config.Provider))
            {
                throw new InvalidOperationException("AI:TextEmbedding:Provider configuration is missing.");
            }

            if (string.Equals(config.Provider, "OpenAI", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(config.Provider, "LiteLLM", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(config.ModelId))
                {
                    throw new InvalidOperationException("AI:TextEmbedding:ModelId is required for OpenAI provider.");
                }
                
                var apiKey = !string.IsNullOrEmpty(config.ApiKey) ? config.ApiKey : "dummy-key";

                if (!string.IsNullOrEmpty(config.Endpoint))
                {
                    var client = new OpenAIClient(new Uri(config.Endpoint), new AzureKeyCredential(apiKey));
                    return new OpenAITextEmbeddingGenerationService(config.ModelId, client);
                }

                return new OpenAITextEmbeddingGenerationService(
                    modelId: config.ModelId,
                    apiKey: apiKey
                );
            }

            return new OllamaEmbeddingService(sp.GetRequiredService<IOptions<AIConfig>>());
        });

        return services;
    }
}
