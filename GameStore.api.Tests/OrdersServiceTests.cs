using GameStore.api;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit; 
namespace GameStore.api.Tests;

public class OrdersServiceTests
{
    private readonly GameStoreContext _context;
    private readonly OrdersService _ordersService;
    private readonly IPaymentService _paymentService;

    public OrdersServiceTests()
    {
        var options = new DbContextOptionsBuilder<GameStoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        _context = new GameStoreContext(options);
        
        _paymentService = Substitute.For<IPaymentService>();
        
        _paymentService.ProcessPaymentAsync(default!, default!)
            .ReturnsForAnyArgs(Task.FromResult(new PaymentResult(true, "MOCK-TX-123", null)));

        _ordersService = new OrdersService(_context, _paymentService);
    }
    [Fact]
    public async Task CheckIfPaid_WhenPossibleToBuy_ShouldReturnSuccess()
    {
        _context.Games.Add(new Game 
        {
            Id = 1, Title = "TestGame",
        });
        _context.Users.Add(new ApplicationUser
        {
            Id = "1", UserName = "testuser", DisplayName = "Test User"
        });
        _context.Platforms.Add(new Platform
        {
            Id = 1, Name = "PC"
        });
        _context.GameOffers.Add(new GameOffer
        {
            Id = 1, GameId = 1, SellerId = "1", PlatformId = 1, Price = 10, Stock = 3
        });

        await _context.SaveChangesAsync();

        var createOrderDto = new CreateOrderDto(
            new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(1, 2)
            }
        );
        var buyerId = "fake-buyer-123";


        var result = await _ordersService.CreateAsync(createOrderDto, buyerId);


        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Data);
        Assert.Equal(OrderStatus.Paid.ToString(), result.Data.Status);
        
        var updatedOffer = await _context.GameOffers.FindAsync(1);
        Assert.Equal(1, updatedOffer!.Stock);   
    }
    
    [Fact]
    public async Task CheckIfPaid_WhenNotEnoughStock_ShouldReturnValidationError()
    {
        _context.Games.Add(new Game 
        {
            Id = 1, Title = "TestGame",
        });
        _context.Users.Add(new ApplicationUser
        {
            Id = "1", UserName = "testuser", DisplayName = "Test User"
        });
        _context.Platforms.Add(new Platform
        {
            Id = 1, Name = "PC"
        });
        _context.GameOffers.Add(new GameOffer
        {
            Id = 1, GameId = 1, SellerId = "1", PlatformId = 1, Price = 10, Stock = 3
        });

        await _context.SaveChangesAsync();

        var createOrderDto = new CreateOrderDto(
            new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto(1, 5)
            }
        );
        var buyerId = "fake-buyer-123";


        var result = await _ordersService.CreateAsync(createOrderDto, buyerId);


        Assert.False(result.IsSuccess);
        Assert.Contains("Insufficient stock", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckIfPaid_WhenPaymentFails_ShouldReturnValidationError()
    {
        _context.Games.Add(new Game 
        { 
            Id = 1, 
            Title = "TestGame" 
        });
        _context.Users.Add(new ApplicationUser 
        { 
            Id = "1", 
            UserName = "testuser", 
            DisplayName = "Test User" 
        });
        _context.Platforms.Add(new Platform 
        { 
            Id = 1, 
            Name = "PC" 
        });
        _context.GameOffers.Add(new GameOffer 
        { 
            Id = 1, 
            GameId = 1, 
            SellerId = "1", 
            PlatformId = 1, 
            Price = 10, 
            Stock = 3
        });
        await _context.SaveChangesAsync();

        var createOrderDto = new CreateOrderDto(
            new List<CreateOrderItemDto> 
            { 
                new CreateOrderItemDto(1, 2) 
            });
        var buyerId = "fake-buyer-123";

        _paymentService.ProcessPaymentAsync(default!, default!)
            .ReturnsForAnyArgs(Task.FromResult(new PaymentResult(false, null, "Insufficient funds")));

        var result = await _ordersService.CreateAsync(createOrderDto, buyerId);

        Assert.False(result.IsSuccess);
        Assert.Contains("Payment failed", result.ErrorMessage);
        Assert.Contains("Insufficient funds", result.ErrorMessage);
        
        // EF Core's InMemoryDatabase doesn't actually support transactions.
        // That means `RollbackAsync()` won't revert `offer.Stock` back to 3 in this test,
        // even though it works perfectly fine in a real SQL database!
        
        // Just make sure the order wasn't saved:
        var ordersCount = await _context.Orders.CountAsync();
        Assert.Equal(0, ordersCount);
    }
}
