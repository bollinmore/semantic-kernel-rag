namespace RagMcpServer.Services;

using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.Chroma.Http;

public class ChromaDbService
{
    private readonly ChromaClient _client;

    public ChromaDbService(IConfiguration configuration)
    {
        var endpoint = configuration["ChromaDb:Endpoint"] ?? "http://localhost:8000";
        _client = new ChromaClient(new HttpClient { BaseAddress = new Uri(endpoint) });
    }

    // Methods for interacting with ChromaDB will be added here
}
