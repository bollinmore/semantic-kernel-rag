namespace RagMcpServer.Controllers;

using Microsoft.AspNetCore.Mvc;
using RagMcpServer.Models;
using RagMcpServer.Services;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentProcessingService _documentProcessingService;
    private readonly ChromaDbService _chromaDbService;
    private readonly OllamaEmbeddingService _ollamaEmbeddingService;

    public DocumentsController(DocumentProcessingService documentProcessingService, ChromaDbService chromaDbService, OllamaEmbeddingService ollamaEmbeddingService)
    {
        _documentProcessingService = documentProcessingService;
        _chromaDbService = chromaDbService;
        _ollamaEmbeddingService = ollamaEmbeddingService;
    }

    [HttpPost]
    public async Task<IActionResult> IngestDocuments([FromBody] IngestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
        {
            return BadRequest(new { error = "Path is required." });
        }

        var jobId = Guid.NewGuid().ToString();

        _ = Task.Run(async () =>
        {
            await foreach (var chunk in _documentProcessingService.GetDocumentChunksAsync(request.Path))
            {
                // In a real application, you would add these to ChromaDB
                // after generating embeddings.
                Console.WriteLine($"[Job {jobId}] Processing chunk...");
            }
        });

        return Accepted(new IngestionResponse { JobId = jobId, Status = "Started" });
    }
}
