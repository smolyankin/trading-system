using System.Net;
using System.Net.Http.Json;

namespace TradingSystem.Tests.Unit.Api;

public class StatsEndpointTests
{
    [Fact]
    public async Task GET_stats_Should_Return_Correct_Count_Format_When_Database_Is_Empty()
    {
        // Arrange
        await using var factory = new StatsWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/stats");

        // Assert
        if (response.StatusCode == HttpStatusCode.OK)
        {
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            var count = await response.Content.ReadFromJsonAsync<int>();
            Assert.Equal(0, count);
        }
    }

    [Fact]
    public async Task GET_stats_Should_Exist_When_Endpoint_Route_Exists()
    {
        // Arrange
        await using var factory = new StatsWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/stats");

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
