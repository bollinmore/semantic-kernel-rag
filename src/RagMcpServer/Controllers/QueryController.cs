namespace RagMcpServer.Controllers;

using Microsoft.AspNetCore.Mvc;
using RagMcpServer.Models;
using RagMcpServer.Services;

[ApiController]
[Route("[controller]")]
public class QueryController : ControllerBase
{
    private readonly QueryService _queryService;

    public QueryController(QueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpPost]
    public async Task<IActionResult> Query([FromBody] QueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return BadRequest(new { error = "Query is required." });
        }

        var response = await _queryService.QueryAsync(request.Query, request.IncludeSources);

        if (string.IsNullOrEmpty(response.Answer))
        {
            return NotFound();
        }

        return Ok(response);
    }
}
