using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace RagMcpClient
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var app = new CommandApp();
            app.Configure(config =>
            {
                config.SetApplicationName("rag-cli");
                config.AddCommand<RagMcpClient.Commands.InjectCommand>("inject")
                    .WithDescription("Import documents from a local directory");
                config.AddCommand<RagMcpClient.Commands.QueryCommand>("query")
                    .WithDescription("Ask a question to the system");
                config.AddCommand<RagMcpClient.Commands.HelpCommand>("help")
                    .WithDescription("Show help information");
                config.AddCommand<RagMcpClient.Commands.TestConnectionCommand>("test-connection")
                    .WithDescription("Verify connection to MCP Server");
            });
            return await app.RunAsync(args);
        }
    }
}