namespace RagMcpServer.Controllers;

using Microsoft.AspNetCore.Mvc;
using RagMcpServer.Models;
using RagMcpServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Embeddings;

[ApiController]
[Route("[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ILogger<DocumentsController> _logger;
    private readonly DocumentProcessingService _documentProcessingService;
    private readonly ChromaDbService _chromaDbService;
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public DocumentsController(
        ILogger<DocumentsController> logger,
        DocumentProcessingService documentProcessingService,
        ChromaDbService chromaDbService,
        OllamaEmbeddingService embeddingService)
    {
        _logger = logger;
        _documentProcessingService = documentProcessingService;
        _chromaDbService = chromaDbService;
        _embeddingService = embeddingService;
    }

    [HttpPost]
    public IActionResult IngestDocuments([FromBody] IngestionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path) || (!System.IO.File.Exists(request.Path) && !System.IO.Directory.Exists(request.Path)))
        {
            return BadRequest(new { error = "A valid file or directory path is required." });
        }

        var jobId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting ingestion job {JobId} for path: {Path}", jobId, request.Path);

        _ = Task.Run(async () =>
        {
            var chunks = new List<string>();
            await foreach (var chunk in _documentProcessingService.GetDocumentChunksAsync(request.Path))
            {
                chunks.Add(chunk);
            }

            if (chunks.Any())
            {
                _logger.LogInformation("Job {JobId}: Generating embeddings for {ChunkCount} chunks...", jobId, chunks.Count);
                var embeddings = await _embeddingService.GenerateEmbeddingsAsync(chunks);

                var chunksWithEmbeddings = chunks.Zip(embeddings, (text, embedding) => (text, embedding));

                _logger.LogInformation("Job {JobId}: Saving chunks to vector store...", jobId);
                await _chromaDbService.SaveChunksAsync(chunksWithEmbeddings);
            }

            _logger.LogInformation("Ingestion job {JobId} completed.", jobId);
        });

        return Accepted(new IngestionResponse { JobId = jobId, Status = "Started" });
    }
}