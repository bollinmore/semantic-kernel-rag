using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ChatCompletion;
using RagMcpServer.Middleware;
using RagMcpServer.Services;
using RagMcpServer.Configuration;
using RagMcpServer.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add Configuration
builder.Services.Configure<AIConfig>(builder.Configuration.GetSection(AIConfig.SectionName));

// Add services to the container.
builder.Services.AddSingleton<IVectorDbService, SqliteDbService>();
builder.Services.AddAIServices();
builder.Services.AddSingleton<DocumentProcessingService>();
builder.Services.AddSingleton<QueryService>();

// Add Semantic Kernel
builder.Services.AddKernel();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
