namespace RagMcpServer.Services;

using Microsoft.SemanticKernel;
using RagMcpServer.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Embeddings;

public class QueryService
{
    private readonly ChromaDbService _chromaDbService;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly Kernel _kernel;

    public QueryService(ChromaDbService chromaDbService, OllamaEmbeddingService embeddingService, Kernel kernel)
    {
        _chromaDbService = chromaDbService;
        _embeddingService = embeddingService;
        _kernel = kernel;
    }

    public async Task<QueryResponse> QueryAsync(string query)
    {
        // 1. Get embedding for the query
        var queryEmbedding = (await _embeddingService.GenerateEmbeddingsAsync(new[] { query })).First();

        // 2. Search ChromaDB for relevant documents
        var searchResults = await _chromaDbService.SearchAsync(queryEmbedding, limit: 3);

        if (!searchResults.Any())
        {
            return new QueryResponse { Answer = "No relevant information found in the documents." };
        }

        // 3. Use Semantic Kernel to generate an answer
        var context = string.Join("\n\n", searchResults);

        var prompt = @$"
            You are a helpful AI assistant answering questions based on the context provided.
            Answer the user's question using ONLY the information provided below.
            If the information is not in the context, say ""I don't have enough information to answer.""

            CONTEXT:
            ---
            {context}
            ---

            QUESTION: {query}
            ANSWER:
        ";
        
        var result = await _kernel.InvokePromptAsync(prompt);

        return new QueryResponse
        {
            Answer = result.ToString(),
            SourceDocuments = searchResults.Select(r => new SourceDocument { Content = r }).ToList()
        };
    }
}