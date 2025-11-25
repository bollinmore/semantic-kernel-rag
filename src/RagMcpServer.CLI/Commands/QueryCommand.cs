using System.Threading;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using RagMcpServer.CLI.Services;

namespace RagMcpServer.CLI.Commands;

public class QueryCommand : AsyncCommand<QueryCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<QUERY>")]
        [Description("The question to ask")]
        public string Query { get; set; } = string.Empty;

        [CommandOption("-s|--server <URL>")]
        [Description("URL of the API server")]
        [DefaultValue("http://localhost:5228")]
        public string ServerUrl { get; set; } = "http://localhost:5228";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Query))
        {
            AnsiConsole.MarkupLine("[red]Error: Query text is required.[/]");
            return 1;
        }

        var client = new ApiClient(settings.ServerUrl);

        try
        {
            QueryResponse? result = null;
            await AnsiConsole.Status()
                .StartAsync("Thinking...", async ctx =>
                {
                    result = await client.QueryAsync(settings.Query);
                });

            if (result == null)
            {
                AnsiConsole.MarkupLine("[yellow]No answer found.[/]");
                return 0;
            }

            AnsiConsole.MarkupLine("[bold green]Answer:[/]");
            AnsiConsole.WriteLine(result.Answer);
            AnsiConsole.WriteLine();

            if (result.Sources != null && result.Sources.Count > 0)
            {
                var table = new Table();
                table.AddColumn("Score");
                table.AddColumn("Source");
                table.AddColumn("Snippet");

                foreach (var source in result.Sources)
                {
                    // Escape markup in source content to prevent rendering errors
                    var snippet = source.Snippet.Replace("[", "[[").Replace("]", "]]");
                    var title = source.Title.Replace("[", "[[").Replace("]", "]]");
                    table.AddRow(source.Score.ToString("F2"), title, snippet);
                }

                AnsiConsole.Write(table);
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}
