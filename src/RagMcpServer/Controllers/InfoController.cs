using Microsoft.AspNetCore.Mvc;
using RagMcpServer.Services;
using System.Threading.Tasks;
using RagMcpServer.Models; // Corrected using directive
using Microsoft.Extensions.Options;
using RagMcpServer.Configuration;

namespace RagMcpServer.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController : ControllerBase
{
    private readonly IVectorDbService _vectorDbService;
    private readonly AIConfig _aiConfig;

    public InfoController(IVectorDbService vectorDbService, IOptions<AIConfig> aiConfigOptions)
    {
        _vectorDbService = vectorDbService;
        _aiConfig = aiConfigOptions.Value;
    }

    [HttpGet]
    public async Task<IActionResult> GetInfo()
    {
        var count = await _vectorDbService.GetDocumentCountAsync();
        
        return Ok(new InfoResponse
        {
            VectorDb = new VectorDbInfo
            {
                CollectionName = "rag-collection",
                DocumentCount = count,
                VectorDbType = "Sqlite",
                Algorithm = "Cosine Similarity",
                EmbeddingModelName = _aiConfig.TextEmbedding.ModelId,
                EmbeddingProvider = _aiConfig.TextEmbedding.Provider,
                EmbeddingDimensions = _aiConfig.TextEmbedding.EmbeddingDimensions
            }
        });
    }
}
