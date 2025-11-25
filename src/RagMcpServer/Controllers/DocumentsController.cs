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
    private readonly IVectorDbService _vectorDbService;
    private readonly ITextEmbeddingGenerationService _embeddingService;

    public DocumentsController(
        ILogger<DocumentsController> logger,
        DocumentProcessingService documentProcessingService,
        IVectorDbService vectorDbService,
        ITextEmbeddingGenerationService embeddingService)
    {
        _logger = logger;
        _documentProcessingService = documentProcessingService;
        _vectorDbService = vectorDbService;
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
            var chunks = new List<(string Content, string FilePath)>();
            await foreach (var chunk in _documentProcessingService.GetDocumentChunksAsync(request.Path))
            {
                chunks.Add(chunk);
            }

            if (chunks.Any())
            {
                _logger.LogInformation("Job {JobId}: Generating embeddings for {ChunkCount} chunks...", jobId, chunks.Count);
                // Extract just text for embedding generation
                var texts = chunks.Select(c => c.Content).ToList();
                var embeddings = await _embeddingService.GenerateEmbeddingsAsync(texts);

                // Zip texts, embeddings, AND filePaths
                var chunksWithEmbeddings = chunks.Zip(embeddings, (chunk, embedding) => (chunk.Content, embedding, chunk.FilePath));

                _logger.LogInformation("Job {JobId}: Saving chunks to vector store...", jobId);
                await _vectorDbService.SaveChunksAsync(chunksWithEmbeddings);
            }

            _logger.LogInformation("Ingestion job {JobId} completed.", jobId);
        });

        return Accepted(new IngestionResponse { JobId = jobId, Status = "Started" });
    }
}
