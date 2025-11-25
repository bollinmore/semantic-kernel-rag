using System.Threading;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using RagMcpServer.CLI.Services;

namespace RagMcpServer.CLI.Commands;

public class InjectCommand : AsyncCommand<InjectCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-p|--path <PATH>")]
        [Description("Path to the directory containing documents to import")]
        public string Path { get; set; } = string.Empty;

        [CommandOption("-s|--server <URL>")]
        [Description("URL of the API server")]
        [DefaultValue("http://localhost:5228")]
        public string ServerUrl { get; set; } = "http://localhost:5228";

        [CommandOption("-b|--batch-size <SIZE>")]
        [Description("Number of files to upload in each batch")]
        [DefaultValue(10)]
        public int BatchSize { get; set; } = 10;
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

        var client = new ApiClient(settings.ServerUrl);

        try
        {
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

            // 2. Process in batches with progress bar
            var processedCount = 0;
            var failedCount = 0;
            
            await AnsiConsole.Progress()
                .AutoRefresh(true) // Turn on auto refresh
                .Columns(new ProgressColumn[] 
                {
                    new TaskDescriptionColumn(),    // Task description
                    new ProgressBarColumn(),        // Progress bar
                    new PercentageColumn(),         // Percentage
                    new RemainingTimeColumn(),      // Remaining time
                    new SpinnerColumn(),            // Spinner
                })
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask($"[green]Uploading {files.Count} files...[/]", new ProgressTaskSettings { MaxValue = files.Count });

                    for (int i = 0; i < files.Count; i += settings.BatchSize)
                    {
                        var batch = files.Skip(i).Take(settings.BatchSize).ToList();
                        
                        // Print files in current batch
                        foreach(var file in batch)
                        {
                            AnsiConsole.MarkupLine($"[grey]Uploading:[/] {System.IO.Path.GetFileName(file)}");
                        }

                        try 
                        {
                            var result = await client.UploadFilesAsync(batch);
                            processedCount += result.ProcessedFiles;
                            failedCount += result.FailedFiles;
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Batch failed:[/] {ex.Message}");
                            failedCount += batch.Count;
                        }

                        task.Increment(batch.Count);
                    }
                });

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold green]Import Completed![/]");
            AnsiConsole.MarkupLine($"Total Processed: [green]{processedCount}[/]");
            AnsiConsole.MarkupLine($"Total Failed: [red]{failedCount}[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}