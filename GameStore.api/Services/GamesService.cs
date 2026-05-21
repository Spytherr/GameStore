using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GamesService(GameStoreContext context) : IGamesService
{
    public async Task<List<GameSummaryDto>> GetAllAsync()
    {
        return await context.Games
            .Include(game => game.Genre)
            .Select(game => new GameSummaryDto(
                game.Id,
                game.Title,
                game.Genre!.Name,
                game.Price,
                game.IsOnSale
                    ? Math.Round(game.Price * (1 - game.DiscountPercentage / 100), 2)
                    : null,
                game.IsOnSale,
                game.ReleaseDate
            ))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceResult<GameDetailsDto>> GetByIdAsync(int id)
    {
        var game = await context.Games
            .Include(game => game.Genre)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
            return ServiceResult<GameDetailsDto>.NotFound($"Gra o ID {id} nie została znaleziona.");

        return ServiceResult<GameDetailsDto>.Success(MapToDetailsDto(game));
    }

    public async Task<ServiceResult<GameDetailsDto>> CreateAsync(CreateGameDto dto)
    {
        var genreExists = await context.Genres.AnyAsync(g => g.Id == dto.GenreId);
        if (!genreExists)
            return ServiceResult<GameDetailsDto>.ValidationError($"Gatunek o ID {dto.GenreId} nie istnieje.");

        Game game = new()
        {
            Title = dto.Title,
            Description = dto.Description,
            GenreId = dto.GenreId,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl,
            ReleaseDate = dto.ReleaseDate
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();

        return ServiceResult<GameDetailsDto>.Success(MapToDetailsDto(game));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateGameDto dto)
    {
        var game = await context.Games.FindAsync(id);
        if (game is null)
            return ServiceResult.NotFound($"Gra o ID {id} nie została znaleziona.");

        var genreExists = await context.Genres.AnyAsync(g => g.Id == dto.GenreId);
        if (!genreExists)
            return ServiceResult.ValidationError($"Gatunek o ID {dto.GenreId} nie istnieje.");

        game.Title = dto.Title;
        game.Description = dto.Description;
        game.GenreId = dto.GenreId;
        game.Price = dto.Price;
        game.Stock = dto.Stock;
        game.ImageUrl = dto.ImageUrl;
        game.ReleaseDate = dto.ReleaseDate;

        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var game = await context.Games.FindAsync(id);
        if (game is null)
            return ServiceResult.NotFound($"Gra o ID {id} nie została znaleziona.");

        context.Games.Remove(game);
        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ApplyDiscountAsync(int id, decimal discountPercentage)
    {
        var game = await context.Games.FindAsync(id);
        if (game is null)
            return ServiceResult.NotFound($"Gra o ID {id} nie została znaleziona.");

        if (discountPercentage < 1 || discountPercentage > 90)
            return ServiceResult.ValidationError("Przecena musi być w zakresie od 1% do 90%.");

        game.DiscountPercentage = discountPercentage;
        game.IsOnSale = true;

        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveDiscountAsync(int id)
    {
        var game = await context.Games.FindAsync(id);
        if (game is null)
            return ServiceResult.NotFound($"Gra o ID {id} nie została znaleziona.");

        game.DiscountPercentage = 0;
        game.IsOnSale = false;

        await context.SaveChangesAsync();
        return ServiceResult.Success();
    }

    private static GameDetailsDto MapToDetailsDto(Game game)
    {
        decimal? discountedPrice = game.IsOnSale
            ? Math.Round(game.Price * (1 - game.DiscountPercentage / 100), 2)
            : null;

        return new GameDetailsDto(
            game.Id,
            game.Title,
            game.Description,
            game.GenreId,
            game.Price,
            game.DiscountPercentage,
            discountedPrice,
            game.IsOnSale,
            game.Stock,
            game.ImageUrl,
            game.ReleaseDate
        );
    }
}
