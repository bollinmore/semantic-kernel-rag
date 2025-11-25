using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading.Tasks;
using System.Threading;

namespace RagMcpClient.Commands;

public class HelpCommand : AsyncCommand<HelpCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine("[bold]RagMcpClient[/]");
        AnsiConsole.WriteLine();
        
        AnsiConsole.MarkupLine("[bold]Usage:[/] [silver]dotnet run --[/] [bold]<COMMAND>[/] [silver][[OPTIONS]][/]");
        AnsiConsole.WriteLine();

        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().Width(15));
        grid.AddColumn(new GridColumn());

        grid.AddRow("[bold]inject[/]", "Import documents from a local directory");
        grid.AddRow("[bold]query[/]", "Ask a question to the system");
        grid.AddRow("[bold]info[/]", "Show system information");
        grid.AddRow("[bold]help[/]", "Show this help message");

        AnsiConsole.MarkupLine("[bold]Commands:[/]");
        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("Run '[bold]dotnet run -- [[command]] --help[/]' for more information on a command.");
        
        return Task.FromResult(0);
    }
}
