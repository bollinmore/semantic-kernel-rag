namespace RagMcpServer.IntegrationTests;

using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using RagMcpServer.Models;
using Xunit;

public class QueryControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public QueryControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Post_Query_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/query", new QueryRequest { Query = "test query" });

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 2xx
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
}
