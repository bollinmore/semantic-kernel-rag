using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RagMcpServer.IntegrationTests;

public class DocumentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DocumentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UploadDocuments_ReturnsOk_WhenFilesUploaded()
    {
        // Arrange
        var client = _factory.CreateClient();
        using var content = new MultipartFormDataContent();
        var fileContent = new StringContent("This is a test document content.", Encoding.UTF8, "text/plain");
        content.Add(fileContent, "files", "test.txt");

        // Act
        var response = await client.PostAsync("/Documents/upload", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        
        Assert.True(doc.RootElement.TryGetProperty("processedFiles", out var processed));
        Assert.Equal(1, processed.GetInt32());
    }
}