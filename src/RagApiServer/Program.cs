using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using RagApiServer.Configuration;
using RagApiServer.Extensions;
using RagApiServer.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace RagApiServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add support for --db parameter
                builder.Configuration.AddCommandLine(args, new Dictionary<string, string>
                {
                    { "--db", "VectorDbPath" }
                });

                builder.Host.UseSerilog();

                // Add Configuration
                builder.Services.Configure<AIConfig>(builder.Configuration.GetSection(AIConfig.SectionName));

                // Add services
                builder.Services.AddSingleton<IVectorDbService, SqliteDbService>();
                builder.Services.AddAIServices();
                builder.Services.AddSingleton<DocumentProcessingService>();
                builder.Services.AddSingleton<QueryService>();
                
                // Add Semantic Kernel
                builder.Services.AddKernel();

                // Add FluentValidation
                builder.Services.AddFluentValidationAutoValidation();
                builder.Services.AddValidatorsFromAssemblyContaining<Program>();

                // Add Swagger
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                // Add Controllers
                builder.Services.AddControllers();

                var app = builder.Build();

                // Configure pipeline
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        
                        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                        
                        if (exceptionHandlerPathFeature?.Error != null)
                        {
                            Log.Error(exceptionHandlerPathFeature.Error, "Unhandled exception during request processing");
                        }

                        await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error" });
                    });
                });

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }
                else 
                {
                    // Allow Swagger in production for this demo/tool if needed, or keep dev-only. 
                    // Spec said "Enable Swagger", didn't restrict to Dev. Let's enable it generally for easy access.
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.MapControllers();

                Log.Information("RagApiServer starting...");

                await app.RunAsync();
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