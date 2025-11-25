using Microsoft.AspNetCore.Mvc;
using RagMcpServer.Services;
using System.Threading.Tasks;

namespace RagMcpServer.Controllers;

[ApiController]
[Route("[controller]")]
public class InfoController : ControllerBase
{
    private readonly IVectorDbService _vectorDbService;

    public InfoController(IVectorDbService vectorDbService)
    {
        _vectorDbService = vectorDbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetInfo()
    {
        var count = await _vectorDbService.GetDocumentCountAsync();
        return Ok(new
        {
            VectorDb = new
            {
                CollectionName = "rag-collection",
                DocumentCount = count,
                Provider = "Sqlite"
            }
        });
    }
}
