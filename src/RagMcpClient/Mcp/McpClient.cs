using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;

namespace RagMcpClient.Mcp;

public class McpClient : IDisposable
{
    private Process? _serverProcess;
    private StreamWriter? _stdin;
    private StreamReader? _stdout;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private int _messageIdCounter = 0;

    public async Task StartAsync(string serverPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = serverPath,
            // Arguments = "", // Add arguments if needed
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true, // We want to see logs
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _serverProcess = new Process { StartInfo = startInfo };
        
        // Forward stderr to console so we can see server logs
        _serverProcess.ErrorDataReceived += (sender, e) => 
        {
            if (!string.IsNullOrEmpty(e.Data))
                AnsiConsole.MarkupLine($"[grey][[Server Log]] {Markup.Escape(e.Data)}[/]");
        };

        try
        {
            if (!_serverProcess.Start())
            {
                throw new Exception("Failed to start server process.");
            }
            _serverProcess.BeginErrorReadLine();

            _stdin = _serverProcess.StandardInput;
            _stdout = _serverProcess.StandardOutput;

            // Handshake
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error starting server: {ex.Message}[/]");
            throw;
        }
    }

    private async Task InitializeAsync()
    {
        var request = new McpRequest
        {
            Id = GetNextId(),
            Method = "initialize",
            Params = new { clientInfo = new { name = "RagMcpClient", version = "1.0.0" } }
        };

        var response = await SendRequestAsync(request);
        if (response?.Error != null)
        {
            throw new Exception($"Initialization failed: {response.Error.Message}");
        }
        // Can check result here if needed
    }

    public async Task<McpResponse?> SendRequestAsync(McpRequest request)
    {
        if (_serverProcess == null || _serverProcess.HasExited)
        {
            throw new Exception("Server is not running.");
        }

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        await _stdin!.WriteLineAsync(json);
        await _stdin.FlushAsync();

        // Read response
        // Note: This is a simple request-response lock-step implementation.
        // A real robust client handles async notifications and interleaved messages.
        var line = await _stdout!.ReadLineAsync();
        if (string.IsNullOrEmpty(line)) return null;

        return JsonSerializer.Deserialize<McpResponse>(line, _jsonOptions);
    }

    public async Task<List<McpTool>> ListToolsAsync()
    {
        var request = new McpRequest
        {
            Id = GetNextId(),
            Method = "tools/list"
        };
        
        var response = await SendRequestAsync(request);
        if (response?.Result == null) return new List<McpTool>();

        // Deserialize result to get tools list
        // Assuming Result structure: { "tools": [...] }
        var jsonResult = JsonSerializer.Serialize(response.Result);
        using var doc = JsonDocument.Parse(jsonResult);
        if (doc.RootElement.TryGetProperty("tools", out var toolsElement))
        {
             return JsonSerializer.Deserialize<List<McpTool>>(toolsElement.GetRawText(), _jsonOptions) ?? new List<McpTool>();
        }
        return new List<McpTool>();
    }

    public async Task<McpResponse?> CallToolAsync(string toolName, object arguments)
    {
        var request = new McpRequest
        {
            Id = GetNextId(),
            Method = "tools/call",
            Params = new 
            {
                name = toolName,
                arguments = arguments
            }
        };

        return await SendRequestAsync(request);
    }

    private int GetNextId() => Interlocked.Increment(ref _messageIdCounter);

    public void Dispose()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            try { _serverProcess.Kill(); } catch {}
            _serverProcess.Dispose();
        }
    }
}
