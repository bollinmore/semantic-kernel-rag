using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace RagMcpServer.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.SetApplicationName("rag-cli");
                config.AddCommand<RagMcpServer.CLI.Commands.InjectCommand>("inject")
                    .WithDescription("Import documents from a local directory");
                config.AddCommand<RagMcpServer.CLI.Commands.QueryCommand>("query")
                    .WithDescription("Ask a question to the system");
                config.AddCommand<RagMcpServer.CLI.Commands.InfoCommand>("info")
                    .WithDescription("Show system information");
                config.AddCommand<RagMcpServer.CLI.Commands.HelpCommand>("help")
                    .WithDescription("Show help information");
            });
            return await app.RunAsync(args);
        }
    }
}