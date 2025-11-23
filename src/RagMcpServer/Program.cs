using Microsoft.SemanticKernel.Embeddings;
using RagMcpServer.Middleware;
using RagMcpServer.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.AddSingleton<ChromaDbService>();
builder.Services.AddSingleton<OllamaEmbeddingService>();
builder.Services.AddSingleton<ITextEmbeddingGenerationService>(sp => sp.GetRequiredService<OllamaEmbeddingService>());
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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
