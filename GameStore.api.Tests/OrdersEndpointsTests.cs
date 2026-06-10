using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameStore.api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GameStore.api.Tests;

public class OrdersEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrdersEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateOrder_WithoutJwtToken_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();

        var createOrderDto = new CreateOrderDto(
            new List<CreateOrderItemDto> { new CreateOrderItemDto(1, 2) }
        );

        var response = await client.PostAsJsonAsync("/orders", createOrderDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithTestAuthHandler_ShouldPassAuthorization()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<AuthenticationOptions>(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                });

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var createOrderDto = new CreateOrderDto(
            new List<CreateOrderItemDto> { new CreateOrderItemDto(1, 2) }
        );

        var response = await client.PostAsJsonAsync("/orders", createOrderDto);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
