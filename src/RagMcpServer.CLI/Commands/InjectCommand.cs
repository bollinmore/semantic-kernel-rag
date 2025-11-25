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
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Path))
        {
            AnsiConsole.MarkupLine("[red]Error: Path is required.[/]");
            return 1;
        }

        var client = new ApiClient(settings.ServerUrl);

        try
        {
            await AnsiConsole.Status()
                .StartAsync("Uploading documents...", async ctx =>
                {
                    var result = await client.UploadDocumentsAsync(settings.Path);
                    AnsiConsole.MarkupLine($"[green]Success:[/] {result}");
                });
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}
