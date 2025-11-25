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
                await ProcessChunksAsync(jobId, chunks);
            }

            _logger.LogInformation("Ingestion job {JobId} completed.", jobId);
        });

        return Accepted(new IngestionResponse { JobId = jobId, Status = "Started" });
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocuments(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { error = "No files received." });
        }

        var processedCount = 0;
        var failedCount = 0;
        var allChunks = new List<(string Content, string FilePath)>();

        foreach (var file in files)
        {
            try
            {
                if (file.Length > 0)
                {
                    using var stream = file.OpenReadStream();
                    await foreach (var chunk in _documentProcessingService.GetDocumentChunksFromStreamAsync(stream, file.FileName))
                    {
                        allChunks.Add(chunk);
                    }
                    processedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file: {FileName}", file.FileName);
                failedCount++;
            }
        }

        if (allChunks.Any())
        {
            // Process chunks in background or await here? 
            // For upload endpoint, it's often better to await so client knows it's done, 
            // but for large batches background is better. 
            // Given the requirement "summary after import", let's await the storage part 
            // or at least start it and return success count.
            // We will await here to ensure data is in DB when CLI returns.
            await ProcessChunksAsync("upload-" + Guid.NewGuid(), allChunks);
        }

        return Ok(new 
        { 
            processedFiles = processedCount, 
            failedFiles = failedCount, 
            message = $"Successfully processed {processedCount} files." 
        });
    }

    private async Task ProcessChunksAsync(string jobId, List<(string Content, string FilePath)> chunks)
    {
        _logger.LogInformation("Job {JobId}: Generating embeddings for {ChunkCount} chunks...", jobId, chunks.Count);
        var texts = chunks.Select(c => c.Content).ToList();
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(texts);

        var chunksWithEmbeddings = chunks.Zip(embeddings, (chunk, embedding) => (chunk.Content, embedding, chunk.FilePath));

        _logger.LogInformation("Job {JobId}: Saving chunks to vector store...", jobId);
        await _vectorDbService.SaveChunksAsync(chunksWithEmbeddings);
    }
}
