using GameStore.api;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit; 

namespace GameStore.api.Tests;

public class GameOffersServiceTests
{
    private readonly GameStoreContext _context;
    private readonly GameOffersService _gameOffersService;

    public GameOffersServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameStoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new GameStoreContext(options);
        _gameOffersService = new GameOffersService(_context);
    }
    [Theory]
    [InlineData(10, true)]
    [InlineData(0, false)]
    [InlineData(-5, false)]
    [InlineData(100, false)]
    [InlineData(90, true)]
    public async Task ApplyDiscount_ShouldValidatePercentage(int discount, bool expectedSuccess)
    {
        var offer = new GameOffer
        {
            Id = 1,
            GameId = 1,
            SellerId = "seller-123",
            PlatformId = 1,
            Price = 100,
            Stock = 5
        };
        _context.GameOffers.Add(offer);
        await _context.SaveChangesAsync();

        var result = await _gameOffersService.ApplyDiscountAsync(offer.GameId, offer.Id, discount, offer.SellerId);

        Assert.Equal(expectedSuccess, result.IsSuccess);
    }

    [Fact]
    public async Task ApplyDiscount_WhenOfferDoesNotExist_ShouldReturnNotFound()
    {
        var result = await _gameOffersService.ApplyDiscountAsync(999, 999, 10, "seller-123");
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ApplyDiscount_WhenUserIsNotSeller_ShouldReturnUnauthorized()
    {
        var offer = new GameOffer
        {
            Id = 1,
            GameId = 1,
            SellerId = "seller-123",
            PlatformId = 1,
            Price = 100,
            Stock = 5
        };
        _context.GameOffers.Add(offer);
        await _context.SaveChangesAsync();

        var result = await _gameOffersService.ApplyDiscountAsync(offer.GameId, offer.Id, 10, "other-user-456");

        Assert.False(result.IsSuccess);
        Assert.Contains("permission", result.ErrorMessage);
    }
}
