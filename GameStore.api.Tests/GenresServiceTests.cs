using GameStore.api;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GameStore.api.Tests;

public class GenresServiceTests
{
    private readonly GameStoreContext _context;
    private readonly GenresService _genresService;

    public GenresServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameStoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GameStoreContext(options);
        
        _genresService = new GenresService(_context);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGenres()
    {
        _context.Genres.Add(new Genre { Id = 1, Name = "Action" });
        _context.Genres.Add(new Genre { Id = 2, Name = "RPG" });
        await _context.SaveChangesAsync();

        var result = await _genresService.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Name == "Action");
    }

    [Fact]
    public async Task GetByIdAsync_WhenGenreExists_ShouldReturnSuccessWithGenre()
    {
        var expectedGenre = new Genre { Id = 1, Name = "Strategy" };
        _context.Genres.Add(expectedGenre);
        await _context.SaveChangesAsync();

        var result = await _genresService.GetByIdAsync(1);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Strategy", result.Data.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGenreDoesNotExist_ShouldReturnNotFound()
    {

        var result = await _genresService.GetByIdAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage);
    }
}
