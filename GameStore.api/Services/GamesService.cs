using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GamesService(GameStoreContext context) : IGamesService
{
    public async Task<PagedResult<GameSummaryDto>> GetAllAsync(GamesQueryDto query)
    {
        IQueryable<Game> gamesQuery = context.Games
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Offers);

        if (query.GenreId.HasValue)
            gamesQuery = gamesQuery.Where(g => g.Genres.Any(genre => genre.Id == query.GenreId));

        if (!string.IsNullOrWhiteSpace(query.Search))
            gamesQuery = gamesQuery.Where(g => g.Title.Contains(query.Search));

        gamesQuery = query.SortBy.ToLower() switch
        {
            "releasedate" => query.SortOrder.ToLower() == "desc"
                ? gamesQuery.OrderByDescending(g => g.ReleaseDate)
                : gamesQuery.OrderBy(g => g.ReleaseDate),
            "price" => query.SortOrder.ToLower() == "desc"
                ? gamesQuery.OrderByDescending(g => g.Offers.Any()
                    ? g.Offers.Min(o => o.Price) : decimal.MaxValue)
                : gamesQuery.OrderBy(g => g.Offers.Any()
                    ? g.Offers.Min(o => o.Price) : decimal.MaxValue),
            _ => query.SortOrder.ToLower() == "desc"
                ? gamesQuery.OrderByDescending(g => g.Title)
                : gamesQuery.OrderBy(g => g.Title)
        };

        var totalCount = await gamesQuery.CountAsync();

        var items = await gamesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(game => new GameSummaryDto(
                game.Id,
                game.Title,
                game.Genres.Select(g => g.Name).ToList(),
                game.Platforms.Select(p => p.Name).ToList(),
                game.ImageUrl,
                game.Offers.Any()
                    ? game.Offers.Min(o => o.IsOnSale
                        ? Math.Round(o.Price * (1 - o.DiscountPercentage / 100), 2)
                        : o.Price)
                    : null,
                game.Offers.Any(o => o.IsOnSale),
                game.Offers.Count,
                game.Rating,
                game.ReleaseDate
            ))
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<GameSummaryDto>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<ServiceResult<GameDetailsDto>> GetByIdAsync(int id)
    {
        var game = await context.Games
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .Include(g => g.Offers)
                .ThenInclude(o => o.Seller)
            .Include(g => g.Offers)
                .ThenInclude(o => o.Platform)
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
                o.Stock,
                o.Platform?.Name ?? "Unknown"
            );
        }).ToList();

        return ServiceResult<GameDetailsDto>.Success(new GameDetailsDto(
            game.Id,
            game.Title,
            game.Description,
            game.Genres.Select(g => g.Name).ToList(),
            game.Platforms.Select(p => p.Name).ToList(),
            game.ImageUrl,
            game.ReleaseDate,
            game.Rating,
            offerDtos
        ));
    }

    public async Task<ServiceResult<GameDetailsDto>> CreateAsync(CreateGameDto dto)
    {
        var titleExists = await context.Games.AnyAsync(g => g.Title == dto.Title);
        if (titleExists)
            return ServiceResult<GameDetailsDto>.Conflict(
                $"A game with the title \"{dto.Title}\" already exists in the catalog.");

        var genres = await context.Genres.Where(g => dto.GenreIds.Contains(g.Id)).ToListAsync();
        if (genres.Count != dto.GenreIds.Count)
            return ServiceResult<GameDetailsDto>.ValidationError(
                "One or more provided Genre IDs do not exist.");

        var platforms = await context.Platforms.Where(p => dto.PlatformIds.Contains(p.Id)).ToListAsync();
        if (platforms.Count != dto.PlatformIds.Count)
            return ServiceResult<GameDetailsDto>.ValidationError(
                "One or more provided Platform IDs do not exist.");

        Game game = new()
        {
            Title = dto.Title,
            Description = dto.Description,
            Genres = genres,
            Platforms = platforms,
            ImageUrl = dto.ImageUrl,
            ReleaseDate = dto.ReleaseDate
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();

        return ServiceResult<GameDetailsDto>.Success(new GameDetailsDto(
            game.Id,
            game.Title,
            game.Description,
            game.Genres.Select(g => g.Name).ToList(),
            game.Platforms.Select(p => p.Name).ToList(),
            game.ImageUrl,
            game.ReleaseDate,
            game.Rating,
            []
        ));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateGameDto dto)
    {
        var game = await context.Games
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
            return ServiceResult.NotFound($"Game with ID {id} was not found.");

        var genres = await context.Genres.Where(g => dto.GenreIds.Contains(g.Id)).ToListAsync();
        if (genres.Count != dto.GenreIds.Count)
            return ServiceResult.ValidationError("One or more provided Genre IDs do not exist.");

        var platforms = await context.Platforms.Where(p => dto.PlatformIds.Contains(p.Id)).ToListAsync();
        if (platforms.Count != dto.PlatformIds.Count)
            return ServiceResult.ValidationError("One or more provided Platform IDs do not exist.");

        game.Title = dto.Title;
        game.Description = dto.Description;
        game.Genres = genres;
        game.Platforms = platforms;
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
