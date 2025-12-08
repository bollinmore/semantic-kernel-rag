using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RagMcpServer.Services; // Ensure access to Services

namespace RagMcpServer.Mcp;

public class McpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly List<McpTool> _tools = new();
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    
    // Services (Phase 3 & 4)
    private readonly DocumentProcessingService _documentService;
    private readonly QueryService _queryService;
    private readonly IVectorDbService _vectorDbService;

    public McpServer(ILogger<McpServer> logger, DocumentProcessingService docService, QueryService queryService, IVectorDbService vectorDbService)
    {
        _logger = logger;
        _documentService = docService;
        _queryService = queryService;
        _vectorDbService = vectorDbService;
        RegisterTools();
    }

    private void RegisterTools()
    {
        // Phase 3: Inject Tool
        _tools.Add(new McpTool
        {
            Name = "Inject",
            Description = "Ingest a text document into the vector database.",
            InputSchema = new
            {
                type = "object",
                properties = new
                {
                    text = new { type = "string", description = "The text content to ingest." },
                    collection = new { type = "string", description = "Target collection (default: rag-collection)." },
                    metadata = new { type = "object", description = "Optional metadata." }
                },
                required = new[] { "text" }
            }
        });
        
        // Phase 4: Query Tool
        _tools.Add(new McpTool
        {
            Name = "Query",
            Description = "Retrieve relevant text chunks from the vector database.",
            InputSchema = new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string", description = "The search query." },
                    limit = new { type = "integer", description = "Max results (default: 3)." },
                    collection = new { type = "string", description = "Target collection." }
                },
                required = new[] { "query" }
            }
        });
    }


    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MCP Server listening on stdio...");
        
        using var stdin = Console.OpenStandardInput();
        using var reader = new StreamReader(stdin);
        // We write to stdout directly
        using var stdout = Console.OpenStandardOutput();

        // Simple line-based JSON-RPC reader for this MVP
        // NOTE: A robust implementation would handle Content-Length headers if strictly following full MCP/LSP spec,
        // but simple JSON-RPC often uses line-delimited JSON. We assume line-delimited for this iteration.
        
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null && !cancellationToken.IsCancellationRequested)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line, _jsonOptions);
                if (request == null) continue;

                var response = await HandleRequestAsync(request);
                
                if (response != null)
                {
                    var json = JsonSerializer.Serialize(response, _jsonOptions);
                    Console.WriteLine(json); // Write line to stdout
                    Console.Out.Flush();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");
                // Send error response if possible
            }
        }
    }

    private async Task<McpResponse?> HandleRequestAsync(McpRequest request)
    {
        _logger.LogInformation("Received method: {Method}", request.Method);

        try 
        {
            switch (request.Method)
            {
                case "initialize":
                    // Allow initialization even if DB doesn't exist yet, as 'Inject' will create it.
                    return new McpResponse 
                    { 
                        Id = request.Id, 
                        Result = new 
                        { 
                            protocolVersion = "2024-11-05", 
                            serverInfo = new { name = "RagMcpServer", version = "1.0.0" },
                            capabilities = new { tools = new { } }
                        } 
                    };
                
                case "tools/list":
                    return new McpResponse 
                    { 
                        Id = request.Id, 
                        Result = new { tools = _tools } 
                    };

                case "tools/call":
                    var callParams = JsonSerializer.Deserialize<McpToolCallParams>(request.Params?.ToString() ?? "{}", _jsonOptions);
                    if (callParams == null || string.IsNullOrEmpty(callParams.Name))
                         return new McpResponse { Id = request.Id, Error = new McpError { Code = -32602, Message = "Missing tool name" } };

                    if (callParams.Name.Equals("Inject", StringComparison.OrdinalIgnoreCase))
                    {
                        if (callParams.Arguments.TryGetProperty("text", out var textElement))
                        {
                            var text = textElement.GetString();
                            if (string.IsNullOrEmpty(text))
                                return new McpResponse { Id = request.Id, Error = new McpError { Code = -32602, Message = "Text argument is required" } };
                            
                            // Extract metadata if present
                            // Simple handling: pass filename if in metadata, else "unknown"
                            var filePath = "mcp-ingest";
                            if (callParams.Arguments.TryGetProperty("metadata", out var metaElement) && metaElement.TryGetProperty("filename", out var fileElement))
                            {
                                filePath = fileElement.GetString() ?? filePath;
                            }

                            // Process
                            // Assuming DocumentProcessingService.ProcessAndSaveAsync(text, filePath) exists
                            await _documentService.ProcessAndSaveAsync(text, filePath);

                            return new McpResponse 
                            { 
                                Id = request.Id, 
                                Result = new { content = new[] { new { type = "text", text = "Injection successful" } } } 
                            };
                        }
                        else
                        {
                            return new McpResponse { Id = request.Id, Error = new McpError { Code = -32602, Message = "Missing 'text' argument" } };
                        }
                    }
                    
                    if (callParams.Name.Equals("Query", StringComparison.OrdinalIgnoreCase))
                    {
                        if (callParams.Arguments.TryGetProperty("query", out var queryElement))
                        {
                            var query = queryElement.GetString();
                            if (string.IsNullOrEmpty(query))
                                return new McpResponse { Id = request.Id, Error = new McpError { Code = -32602, Message = "Query argument is required" } };
                            
                            var limit = 3;
                            if (callParams.Arguments.TryGetProperty("limit", out var limitElement) && limitElement.TryGetInt32(out var l))
                            {
                                limit = l;
                            }

                            // Process
                            var results = await _queryService.SearchAsync(query, limit);

                            // Format results as a JSON string or structure. 
                            // The spec says 'content' list. We can serialize results into a text block or structure.
                            // To match spec SC-003 "Server returns relevant text chunks", we return them in the 'content'.
                            // However, MCP 'content' is for LLM consumption.
                            // Let's return a JSON string of results as the text.
                            var jsonResults = JsonSerializer.Serialize(results, _jsonOptions);
                            
                            return new McpResponse 
                            { 
                                Id = request.Id, 
                                Result = new { content = new[] { new { type = "text", text = jsonResults } } } 
                            };
                        }
                        else
                        {
                            return new McpResponse { Id = request.Id, Error = new McpError { Code = -32602, Message = "Missing 'query' argument" } };
                        }
                    }

                    return new McpResponse { Id = request.Id, Error = new McpError { Code = -32601, Message = $"Tool not found: {callParams.Name}" } };

                default:
                    return new McpResponse { Id = request.Id, Error = new McpError { Code = -32601, Message = "Method not found" } };
            }
        }
        catch (Exception ex)
        {
            return new McpResponse { Id = request.Id, Error = new McpError { Code = -32603, Message = ex.Message } };
        }
    }
}
