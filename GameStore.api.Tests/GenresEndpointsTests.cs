using System.Net;
using System.Net.Http.Json;
using GameStore.api;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GameStore.api.Tests;

public class GenresEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GenresEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetGenres_ShouldReturnOkStatus()
    {
        var response = await _client.GetAsync("/genres");

        response.EnsureSuccessStatusCode(); 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var genres = await response.Content.ReadFromJsonAsync<List<GenreDto>>();
        Assert.NotNull(genres);
    }
}
