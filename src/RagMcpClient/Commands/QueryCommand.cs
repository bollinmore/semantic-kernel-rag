using System.Threading;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using RagMcpClient.Services;
using RagMcpClient.Mcp;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RagMcpClient.Commands;

public class QueryCommand : AsyncCommand<QueryCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<QUERY>")]
        [Description("The question to ask")]
        public string Query { get; set; } = string.Empty;

        [CommandOption("-s|--server-path <PATH>")]
        [Description("Path to RagMcpServer executable")]
        public string? ServerPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Query))
        {
            AnsiConsole.MarkupLine("[red]Error: Query text is required.[/]");
            return 1;
        }

        // Config setup
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        var config = builder.Build();

        // Server Path Logic
        var serverPath = settings.ServerPath;
        if (string.IsNullOrEmpty(serverPath))
        {
            var baseDir = AppContext.BaseDirectory;
            var candidate = System.IO.Path.Combine(baseDir, "../../../../../RagMcpServer/bin/Debug/net10.0/RagMcpServer");
            if (!File.Exists(candidate)) candidate += ".exe";
            if (!File.Exists(candidate))
            {
                candidate = "src/RagMcpServer/bin/Debug/net10.0/RagMcpServer";
                 if (!File.Exists(candidate)) candidate += ".exe";
            }
            if (File.Exists(candidate)) serverPath = System.IO.Path.GetFullPath(candidate);
        }
        
        if (string.IsNullOrEmpty(serverPath) || !File.Exists(serverPath))
        {
             AnsiConsole.MarkupLine("[red]Could not locate RagMcpServer. Please build it and provide path via --server-path.[/]");
             return 1;
        }

        var client = new McpClient();
        var agent = new AgentService(config);

        try
        {
            await client.StartAsync(serverPath);

            string answer = "";
            await AnsiConsole.Status()
                .StartAsync("Thinking...", async ctx =>
                {
                    ctx.Status(" querying MCP server...");
                    answer = await agent.ProcessQueryAsync(client, settings.Query);
                });

            AnsiConsole.MarkupLine("[bold green]Answer:[/]");
            AnsiConsole.WriteLine(answer);
            AnsiConsole.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
        finally
        {
            client.Dispose();
        }
    }
}
