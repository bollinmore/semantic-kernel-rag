using System.Threading;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using RagMcpClient.Mcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace RagMcpClient.Commands;

public class InjectCommand : AsyncCommand<InjectCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<PATH>")]
        [Description("Path to the directory containing documents to import")]
        public string Path { get; set; } = string.Empty;

        [CommandOption("-s|--server-path <PATH>")]
        [Description("Path to RagMcpServer executable")]
        public string? ServerPath { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Path))
        {
            AnsiConsole.MarkupLine("[red]Error: Path is required.[/]");
            return 1;
        }

        if (!Directory.Exists(settings.Path))
        {
            AnsiConsole.MarkupLine($"[red]Error: Directory not found: {settings.Path}[/]");
            return 1;
        }

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

        // Auto-detect server path if not provided (reusing logic from TestConnectionCommand, should ideally be shared)
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
            
            if (File.Exists(candidate))
            {
                serverPath = System.IO.Path.GetFullPath(candidate);
                AnsiConsole.MarkupLine($"[grey]Auto-detected server at: {serverPath}[/]");
            }
            else
            {
                 AnsiConsole.MarkupLine("[red]Could not locate RagMcpServer. Please build it and provide path via --server-path.[/]");
                 return 1;
            }
        }

        var client = new McpClient(mcpLogger);

        try
        {
            await client.StartAsync(serverPath);

            // 1. Scan for files
            var files = new List<string>();
            await AnsiConsole.Status().StartAsync("Scanning directory...", async ctx =>
            {
                foreach (var file in Directory.EnumerateFiles(settings.Path, "*.*", SearchOption.AllDirectories))
                {
                    var ext = System.IO.Path.GetExtension(file).ToLower();
                    if (ext == ".txt" || ext == ".md")
                    {
                        files.Add(file);
                    }
                }
                await Task.CompletedTask;
            });

            if (files.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No supported files found (only .txt and .md are supported).[/]");
                return 0;
            }

            AnsiConsole.MarkupLine($"Found [bold]{files.Count}[/] files to import.");

            // 2. Process
            var processedCount = 0;
            var failedCount = 0;
            
            await AnsiConsole.Progress()
                .AutoRefresh(true)
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask($"[green]Injecting {files.Count} files...[/]", new ProgressTaskSettings { MaxValue = files.Count });

                    foreach (var file in files)
                    {
                        var fileName = System.IO.Path.GetFileName(file);
                        AnsiConsole.MarkupLine($"[grey]Injecting:[/] {fileName}");

                        try 
                        {
                            var text = await File.ReadAllTextAsync(file);
                            var response = await client.CallToolAsync("Inject", new 
                            { 
                                text = text,
                                metadata = new { filename = fileName }
                            });

                            if (response?.Error != null)
                            {
                                AnsiConsole.MarkupLine($"[red]Failed to inject {fileName}: {response.Error.Message}[/]");
                                failedCount++;
                            }
                            else
                            {
                                processedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Error processing {fileName}: {ex.Message}[/]");
                            failedCount++;
                        }

                        // Add a small cool-down delay between files to prevent overheating/overloading the local LLM
                        await Task.Delay(200);

                        task.Increment(1);
                    }
                });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold green]Injection Completed![/]");
            AnsiConsole.MarkupLine($"Total Processed: [green]{processedCount}[/]");
            AnsiConsole.MarkupLine($"Total Failed: [red]{failedCount}[/]");

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