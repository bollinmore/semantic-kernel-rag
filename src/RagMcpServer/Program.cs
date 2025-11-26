using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using RagMcpServer.Configuration;
using RagMcpServer.Extensions;
using RagMcpServer.Services;
// McpServer will be added in Phase 2

namespace RagMcpServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configure Serilog to write to stderr to avoid interfering with stdout (JSON-RPC)
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(standardErrorFromLevel: Serilog.Events.LogEventLevel.Verbose) 
                .CreateLogger();

            try
            {
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(AppContext.BaseDirectory);
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureServices((context, services) =>
                    {
                        // Add Configuration
                        services.Configure<AIConfig>(context.Configuration.GetSection(AIConfig.SectionName));

                        // Add services
                        services.AddSingleton<IVectorDbService, SqliteDbService>();
                        services.AddAIServices();
                        services.AddSingleton<DocumentProcessingService>();
                        services.AddSingleton<QueryService>();
                        services.AddKernel();

                        // Register McpServer
                        services.AddSingleton<Mcp.McpServer>(); 
                    })
                    .UseSerilog()
                    .Build();

                Log.Information("RagMcpServer starting...");
                
                // Run the MCP Server Loop
                var mcpServer = host.Services.GetRequiredService<Mcp.McpServer>();
                await mcpServer.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}