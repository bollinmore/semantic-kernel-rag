using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RagApiServer.Services;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace RagApiServer.IntegrationTests;

public class ConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void VectorDbService_ReadsConnectionString_FromConfiguration()
    {
        var tempFile = Path.GetTempFileName();
        var connStr = $"Data Source={tempFile}";

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:Sqlite", connStr }
                });
            });
        });

        using var scope = factory.Services.CreateScope();
        var dbService = scope.ServiceProvider.GetRequiredService<IVectorDbService>();
        
        // Assert
        // The new check for Exists in SqliteDbService relies on Path.GetFullPath, so tempFile is now explicitly checked.
        // We also need to ensure the collection exists for it to be 'true' in a meaningful way for this test, but the point is connection.
        Assert.True(dbService.Exists, "DbService should find the existing temp file.");

        // Clean up
        if (File.Exists(tempFile)) File.Delete(tempFile);
    }
}