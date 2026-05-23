using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GamesService(GameStoreContext context) : IGamesService
{
    public async Task<List<GameSummaryDto>> GetAllAsync()
    {
        return await context.Games
            .Include(g => g.Genre)
            .Include(g => g.Offers)
            .Select(game => new GameSummaryDto(
                game.Id,
                game.Title,
                game.Genre!.Name,
                game.ImageUrl,
                game.Offers.Any()
                    ? game.Offers.Min(o => o.IsOnSale
                        ? Math.Round(o.Price * (1 - o.DiscountPercentage / 100), 2)
                        : o.Price)
                    : null,
                game.Offers.Any(o => o.IsOnSale),
                game.Offers.Count,
                game.ReleaseDate
            ))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceResult<GameDetailsDto>> GetByIdAsync(int id)
    {
        var game = await context.Games
            .Include(g => g.Genre)
            .Include(g => g.Offers)
                .ThenInclude(o => o.Seller)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
            return ServiceResult<GameDetailsDto>.NotFound(
                $"Game with ID {id} was not found.");

        var offerDtos = game.Offers.Select(o =>
        {
            decimal? discountedPrice = o.IsOnSale
                ? Math.Round(o.Price * (1 - o.DiscountPercentage / 100), 2)
                : null;

            return new GameOfferDto(
                o.Id,
                o.GameId,
                o.Seller?.DisplayName ?? "Unknown",
                o.Price,
                discountedPrice,
                o.IsOnSale,
                o.Stock
            );
        }).ToList();

        return ServiceResult<GameDetailsDto>.Success(new GameDetailsDto(
            game.Id,
            game.Title,
            game.Description,
            game.GenreId,
            game.Genre!.Name,
            game.ImageUrl,
            game.ReleaseDate,
            offerDtos
        ));
    }

    public async Task<ServiceResult<GameDetailsDto>> CreateAsync(CreateGameDto dto)
    {
        var titleExists = await context.Games.AnyAsync(g => g.Title == dto.Title);
        if (titleExists)
            return ServiceResult<GameDetailsDto>.Conflict(
                $"A game with the title \"{dto.Title}\" already exists in the catalog.");

        var genreExists = await context.Genres.AnyAsync(g => g.Id == dto.GenreId);
        if (!genreExists)
            return ServiceResult<GameDetailsDto>.ValidationError(
                $"Genre with ID {dto.GenreId} does not exist.");

        Game game = new()
        {
            Title = dto.Title,
            Description = dto.Description,
            GenreId = dto.GenreId,
            ImageUrl = dto.ImageUrl,
            ReleaseDate = dto.ReleaseDate
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();

        await context.Entry(game).Reference(g => g.Genre).LoadAsync();

        return ServiceResult<GameDetailsDto>.Success(new GameDetailsDto(
            game.Id,
            game.Title,
            game.Description,
            game.GenreId,
            game.Genre!.Name,
            game.ImageUrl,
            game.ReleaseDate,
            []
        ));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateGameDto dto)
    {
        var game = await context.Games.FindAsync(id);
        if (game is null)
            return ServiceResult.NotFound($"Game with ID {id} was not found.");

        var genreExists = await context.Genres.AnyAsync(g => g.Id == dto.GenreId);
        if (!genreExists)
            return ServiceResult.ValidationError($"Genre with ID {dto.GenreId} does not exist.");

        game.Title = dto.Title;
        game.Description = dto.Description;
        game.GenreId = dto.GenreId;
        game.ImageUrl = dto.ImageUrl;
        game.ReleaseDate = dto.ReleaseDate;

        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var game = await context.Games
            .Include(g => g.Offers)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
            return ServiceResult.NotFound($"Game with ID {id} was not found.");

        if (game.Offers.Count > 0)
            return ServiceResult.Conflict(
                $"Cannot delete a game that has {game.Offers.Count} active offers.");

        context.Games.Remove(game);
        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }
}
