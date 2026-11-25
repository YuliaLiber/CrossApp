
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

public class OrderApiTests
{
    [Fact]
    public async Task Post_Creates_Order_And_Get_Returns_It()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders",
            new
            {
                customer = "C-TEST",
                lines = new[]
                {
                    new
                    {
                        productId = "P-001",
                        name = "Стіл",
                        price = 1200m,
                        quantity = 2
                    }
                }
            });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var get = await client.GetAsync(response.Headers.Location);

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        using var json = JsonDocument.Parse(
            await get.Content.ReadAsStringAsync());

        Assert.Equal(
            "C-TEST",
            json.RootElement.GetProperty("customer").GetString());

        Assert.Equal(
            2400m,
            json.RootElement.GetProperty("total").GetDecimal());
    }

    [Fact]
    public async Task Invalid_Order_Returns_400()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders",
            new
            {
                customer = "",
                lines = Array.Empty<object>()
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.True(
            json.RootElement.GetProperty("errors").TryGetProperty(
                "lines", out _));
    }

    [Fact]
    public async Task Delete_Order_Then_Get_Returns_404()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var created = await client.PostAsJsonAsync("/api/orders",
            new
            {
                customer = "C-DELETE",
                lines = new[]
                {
                    new
                    {
                        productId = "P-002",
                        name = "Крісло",
                        price = 1500m,
                        quantity = 1
                    }
                }
            });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);

        var deleted = await client.DeleteAsync(created.Headers.Location);

        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var get = await client.GetAsync(created.Headers.Location);

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
