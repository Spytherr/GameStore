using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GameOffersService(GameStoreContext context) : IGameOffersService
{
    public async Task<ServiceResult<List<GameOfferDto>>> GetByGameIdAsync(int gameId)
    {
        var gameExists = await context.Games.AnyAsync(g => g.Id == gameId);
        if (!gameExists)
            return ServiceResult<List<GameOfferDto>>.NotFound(
                $"Game with ID {gameId} was not found.");

        var offers = await context.GameOffers
            .Where(o => o.GameId == gameId)
            .Include(o => o.Seller)
            .Include(o => o.Platform)
            .AsNoTracking()
            .ToListAsync();

        var offerDtos = offers.Select(MapToDto).ToList();
        return ServiceResult<List<GameOfferDto>>.Success(offerDtos);
    }

    public async Task<ServiceResult<GameOfferDto>> CreateAsync(
        int gameId, CreateGameOfferDto dto, string sellerId)
    {
        var game = await context.Games
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game is null)
            return ServiceResult<GameOfferDto>.NotFound(
                $"Game with ID {gameId} was not found.");

        var isPlatformValid = game.Platforms.Any(p => p.Id == dto.PlatformId);
        if (!isPlatformValid)
            return ServiceResult<GameOfferDto>.ValidationError(
                $"This game is not available on platform with ID {dto.PlatformId}.");

        var existingOffer = await context.GameOffers
            .AnyAsync(o => o.GameId == gameId && o.SellerId == sellerId && o.PlatformId == dto.PlatformId);
        
        if (existingOffer)
            return ServiceResult<GameOfferDto>.Conflict(
                "You already have an offer for this game on this platform.");

        var offer = new GameOffer
        {
            GameId = gameId,
            PlatformId = dto.PlatformId,
            SellerId = sellerId,
            Price = dto.Price,
            Stock = dto.Stock
        };

        context.GameOffers.Add(offer);
        await context.SaveChangesAsync();

        await context.Entry(offer).Reference(o => o.Seller).LoadAsync();
        await context.Entry(offer).Reference(o => o.Platform).LoadAsync();
        return ServiceResult<GameOfferDto>.Success(MapToDto(offer));
    }

    public async Task<ServiceResult> UpdateAsync(
        int offerId, UpdateGameOfferDto dto, string sellerId)
    {
        var offer = await context.GameOffers.FindAsync(offerId);
        if (offer is null)
            return ServiceResult.NotFound(
                $"Offer with ID {offerId} was not found.");

        if (offer.SellerId != sellerId)
            return ServiceResult.ValidationError(
                "You do not have permission to edit this offer.");

        offer.Price = dto.Price;
        offer.Stock = dto.Stock;

        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int offerId, string sellerId)
    {
        var offer = await context.GameOffers.FindAsync(offerId);
        if (offer is null)
            return ServiceResult.NotFound(
                $"Offer with ID {offerId} was not found.");

        if (offer.SellerId != sellerId)
            return ServiceResult.ValidationError(
                "You do not have permission to delete this offer.");

        context.GameOffers.Remove(offer);
        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ApplyDiscountAsync(
        int offerId, decimal discountPercentage, string sellerId)
    {
        var offer = await context.GameOffers.FindAsync(offerId);
        if (offer is null)
            return ServiceResult.NotFound(
                $"Offer with ID {offerId} was not found.");

        if (offer.SellerId != sellerId)
            return ServiceResult.ValidationError(
                "You do not have permission to edit this offer.");

        if (discountPercentage < 1 || discountPercentage > 90)
            return ServiceResult.ValidationError(
                "Discount must be between 1% and 90%.");

        offer.DiscountPercentage = discountPercentage;
        offer.IsOnSale = true;

        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveDiscountAsync(int offerId, string sellerId)
    {
        var offer = await context.GameOffers.FindAsync(offerId);
        if (offer is null)
            return ServiceResult.NotFound(
                $"Offer with ID {offerId} was not found.");

        if (offer.SellerId != sellerId)
            return ServiceResult.ValidationError(
                "You do not have permission to edit this offer.");

        offer.DiscountPercentage = 0;
        offer.IsOnSale = false;

        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    private static GameOfferDto MapToDto(GameOffer offer)
    {
        decimal? discountedPrice = offer.IsOnSale
            ? Math.Round(offer.Price * (1 - offer.DiscountPercentage / 100), 2)
            : null;

        return new GameOfferDto(
            offer.Id,
            offer.GameId,
            offer.Seller?.DisplayName ?? "Unknown",
            offer.Price,
            discountedPrice,
            offer.IsOnSale,
            offer.Stock,
            offer.Platform?.Name ?? "Unknown"
        );
    }
}
