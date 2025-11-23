namespace RagMcpServer.IntegrationTests;

using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using RagMcpServer.Models;
using Xunit;

public class DocumentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_Documents_ReturnsAccepted()
    {
        // Arrange
        var client = _factory.CreateClient();
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        await File.WriteAllTextAsync(Path.Combine(tempDir, "test.txt"), "some content");

        // Act
        var response = await client.PostAsJsonAsync("/documents", new IngestionRequest { Path = tempDir });

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 2xx
        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
    }
}
