using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace RagMcpServer.IntegrationTests;

public class InfoControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InfoControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetInfo_ReturnsOkAndCorrectStructure()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Info");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        
        Assert.True(doc.RootElement.TryGetProperty("vectorDb", out var vectorDb));
        Assert.True(vectorDb.TryGetProperty("collectionName", out _));
        Assert.True(vectorDb.TryGetProperty("documentCount", out var countProp));
        Assert.True(countProp.ValueKind == JsonValueKind.Number);
    }
}
