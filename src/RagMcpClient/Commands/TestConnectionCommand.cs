using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using RagMcpClient.Mcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace RagMcpClient.Commands;

public class TestConnectionCommand : AsyncCommand<TestConnectionCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-p|--path <PATH>")]
        [Description("Path to RagMcpServer executable")]
        public string? ServerPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        // Config setup
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var config = builder.Build();

        // Setup Logger
        using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder
                .AddConfiguration(config.GetSection("Logging"))
                .AddConsole();
        });
        var mcpLogger = loggerFactory.CreateLogger<McpClient>();

        // Default logic to find server if not provided
        var serverPath = settings.ServerPath;
        if (string.IsNullOrEmpty(serverPath))
        {
            // Assuming default build location relative to execution
            var baseDir = AppContext.BaseDirectory;
            // Adjust based on where Client is running vs Server build
            // Assuming: src/RagMcpClient/bin/Debug/net10.0/RagMcpClient.dll
            // Server:   src/RagMcpServer/bin/Debug/net10.0/RagMcpServer.dll (or exe)
            
            // Try sibling directory structure first
            var candidate = Path.Combine(baseDir, "../../../../../RagMcpServer/bin/Debug/net10.0/RagMcpServer");
            if (!File.Exists(candidate)) candidate += ".exe"; // Windows
            if (!File.Exists(candidate))
            {
                // Try simpler relative path if run from repo root
                candidate = "src/RagMcpServer/bin/Debug/net10.0/RagMcpServer";
                 if (!File.Exists(candidate)) candidate += ".exe";
            }
            
            if (File.Exists(candidate))
            {
                serverPath = Path.GetFullPath(candidate);
                AnsiConsole.MarkupLine($"[grey]Auto-detected server at: {serverPath}[/]");
            }
            else
            {
                 AnsiConsole.MarkupLine("[red]Could not locate RagMcpServer. Please build it and provide path via --path.[/]");
                 return 1;
            }
        }

        using var client = new McpClient(mcpLogger);
        try
        {
            await AnsiConsole.Status().StartAsync("Connecting to Server...", async ctx => 
            {
                await client.StartAsync(serverPath);
                ctx.Status("Connected! Fetching tools...");
                await Task.Delay(500); // UI visual
            });

            AnsiConsole.MarkupLine("[green]Successfully connected to MCP Server![/]");

            var tools = await client.ListToolsAsync();
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Available Tools ({tools.Count}):[/]");
            
            var table = new Table();
            table.AddColumn("Name");
            table.AddColumn("Description");

            foreach (var tool in tools)
            {
                table.AddRow(tool.Name, tool.Description);
            }
            AnsiConsole.Write(table);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Connection failed: {ex.Message}[/]");
            return 1;
        }
    }
}
