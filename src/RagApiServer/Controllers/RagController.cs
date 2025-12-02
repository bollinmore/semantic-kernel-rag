using Microsoft.AspNetCore.Mvc;
using RagApiServer.Models;
using RagApiServer.Services;
using FluentValidation;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.Logging;

namespace RagApiServer.Controllers;

[ApiController]
[Route("api/rag")]
public class RagController : ControllerBase
{
    private readonly IVectorDbService _vectorDbService;
    private readonly ITextEmbeddingGenerationService _embeddingService;
    private readonly IValidator<SearchRequest> _validator;
    private readonly ILogger<RagController> _logger;

    public RagController(
        IVectorDbService vectorDbService, 
        ITextEmbeddingGenerationService embeddingService,
        IValidator<SearchRequest> validator,
        ILogger<RagController> logger)
    {
        _vectorDbService = vectorDbService;
        _embeddingService = embeddingService;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        _logger.LogInformation("Received search request for collection '{CollectionName}'. Query: '{Query}', TopK: {TopK}, Threshold: {Threshold}", 
            request.CollectionName, request.Query, request.TopK, request.ScoreThreshold);

        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Search request validation failed: {Errors}", string.Join(", ", validationResult.Errors));
            return BadRequest(validationResult.Errors);
        }

        // The collection name is now passed directly to the DB service.
        string targetCollection = request.CollectionName; 

        // Generate embedding
        _logger.LogDebug("Generating embedding for query...");
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(new List<string> { request.Query });
        var queryEmbedding = embeddings[0];
        _logger.LogDebug("Embedding generated.");

        // Search
        _logger.LogDebug("Executing vector search on DB for collection '{TargetCollection}'...", targetCollection);
        var results = await _vectorDbService.SearchAsync(queryEmbedding, targetCollection, request.TopK);
        var rawCount = results.Count();
        _logger.LogInformation("Vector DB returned {Count} raw results from collection '{TargetCollection}'.", rawCount, targetCollection);

        // Filter by ScoreThreshold and Map to DTO
        var resultItems = results
            .Where(r => r.Score >= request.ScoreThreshold)
            .Select(r => new ResultItem
            {
                Content = r.Text,
                Metadata = new Metadata
                {
                    Source = r.FilePath,
                    PageNumber = "0", 
                    ChunkIndex = "0"
                }
            })
            .ToList();

        _logger.LogInformation("Returned {Count} results after filtering by threshold {Threshold}.", resultItems.Count, request.ScoreThreshold);

        if (rawCount > 0 && resultItems.Count == 0)
        {
            _logger.LogWarning("All {RawCount} results were filtered out by threshold {Threshold}. Top score was: {TopScore}", 
                rawCount, request.ScoreThreshold, results.Max(r => r.Score));
        }

        return Ok(new SearchResult
        {
            Count = resultItems.Count,
            Results = resultItems
        });
    }
}