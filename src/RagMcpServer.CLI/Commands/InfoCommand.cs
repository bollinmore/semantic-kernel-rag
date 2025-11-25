using System.Threading;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using RagMcpServer.CLI.Services;

namespace RagMcpServer.CLI.Commands;

public class InfoCommand : AsyncCommand<InfoCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-v|--vector_db")]
        [Description("Show vector database information")]
        public bool VectorDb { get; set; }

        [CommandOption("-s|--server <URL>")]
        [Description("URL of the API server")]
        [DefaultValue("http://localhost:5228")]
        public string ServerUrl { get; set; } = "http://localhost:5228";
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var client = new ApiClient(settings.ServerUrl);

        try
        {
            InfoResponse? info = null;
            await AnsiConsole.Status()
                .StartAsync("Fetching info...", async ctx =>
                {
                    info = await client.GetServerInfoAsync();
                });

            if (info == null)
            {
                AnsiConsole.MarkupLine("[red]Error: Could not retrieve info from server.[/]");
                return 1;
            }

            if (settings.VectorDb)
            {
                var table = new Table();
                table.AddColumn("Property");
                table.AddColumn("Value");

                table.AddRow("Collection Name", info.VectorDb.CollectionName);
                table.AddRow("Document Count", info.VectorDb.DocumentCount.ToString());
                table.AddRow("Vector DB Type", info.VectorDb.VectorDbType);
                table.AddRow("Algorithm", info.VectorDb.Algorithm);
                table.AddRow("Embedding Model", info.VectorDb.EmbeddingModelName);
                table.AddRow("Embedding Provider", info.VectorDb.EmbeddingProvider);
                table.AddRow("Embedding Dimensions", info.VectorDb.EmbeddingDimensions.ToString());

                AnsiConsole.Write(table);
            }
            else
            {
                // Default info view if flags not specified (can extend later)
                 AnsiConsole.MarkupLine($"Connected to server at {settings.ServerUrl}");
                 AnsiConsole.MarkupLine($"Vector DB Provider: {info.VectorDb.VectorDbType}"); // Updated
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
