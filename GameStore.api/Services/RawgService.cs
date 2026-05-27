using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class RawgService(
    HttpClient httpClient,
    GameStoreContext context,
    IConfiguration configuration) : IRawgService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private string ApiKey => configuration["Rawg:ApiKey"]!;

    public async Task<ServiceResult<List<RawgGameSearchResultDto>>> SearchAsync(string query)
    {
        var response = await httpClient.GetAsync(
            $"/api/games?search={Uri.EscapeDataString(query)}&key={ApiKey}&page_size=10");

        if (!response.IsSuccessStatusCode)
            return ServiceResult<List<RawgGameSearchResultDto>>.ValidationError(
                "Failed to fetch data from RAWG API.");

        var content = await response.Content.ReadAsStreamAsync();
        var result = await JsonSerializer.DeserializeAsync<RawgSearchResponseDto>(
            content, JsonOptions);

        return ServiceResult<List<RawgGameSearchResultDto>>.Success(
            result?.Results ?? []);
    }

    public async Task<ServiceResult<GameDetailsDto>> ImportAsync(int rawgId)
    {
        var existingGame = await context.Games
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.RawgId == rawgId);

        if (existingGame is not null)
            return ServiceResult<GameDetailsDto>.Conflict(
                $"Game with RAWG ID {rawgId} has already been imported as \"{existingGame.Title}\".");

        var response = await httpClient.GetAsync(
            $"/api/games/{rawgId}?key={ApiKey}");

        if (!response.IsSuccessStatusCode)
            return ServiceResult<GameDetailsDto>.NotFound(
                $"Game with RAWG ID {rawgId} was not found on RAWG.");

        var content = await response.Content.ReadAsStreamAsync();
        var rawgGame = await JsonSerializer.DeserializeAsync<RawgGameDetailsDto>(
            content, JsonOptions);

        if (rawgGame is null)
            return ServiceResult<GameDetailsDto>.ValidationError(
                "Failed to parse RAWG API response.");

        var titleExists = await context.Games.AnyAsync(g => g.Title == rawgGame.Name);
        if (titleExists)
            return ServiceResult<GameDetailsDto>.Conflict(
                $"A game with the title \"{rawgGame.Name}\" already exists in the catalog.");

        var gameGenres = await ResolveGenresAsync(rawgGame.Genres);
        var gamePlatforms = await ResolvePlatformsAsync(rawgGame.Platforms);

        var creatorsString = rawgGame.Developers is not null && rawgGame.Developers.Count > 0
            ? string.Join(", ", rawgGame.Developers.Select(d => d.Name))
            : null;

        var publishersString = rawgGame.Publishers is not null && rawgGame.Publishers.Count > 0
            ? string.Join(", ", rawgGame.Publishers.Select(p => p.Name))
            : null;

        var game = new Game
        {
            Title = rawgGame.Name,
            Creators = creatorsString,
            Publishers = publishersString,
            Genres = gameGenres,
            Platforms = gamePlatforms,
            ImageUrl = rawgGame.BackgroundImage,
            ReleaseDate = ParseReleaseDate(rawgGame.Released),
            RawgId = rawgId,
            Rating = rawgGame.Rating
        };

        context.Games.Add(game);
        await context.SaveChangesAsync();

        return ServiceResult<GameDetailsDto>.Success(new GameDetailsDto(
            game.Id,
            game.Title,
            game.Creators,
            game.Publishers,
            game.Genres.Select(g => g.Name).ToList(),
            game.Platforms.Select(p => p.Name).ToList(),
            game.ImageUrl,
            game.ReleaseDate,
            game.Rating,
            []
        ));
    }

    private async Task<List<Genre>> ResolveGenresAsync(List<RawgGenreDto>? rawgGenres)
    {
        if (rawgGenres is null || rawgGenres.Count == 0)
            return [];

        var genres = new List<Genre>();
        foreach (var rg in rawgGenres)
        {
            var genre = await context.Genres.FirstOrDefaultAsync(g => g.Name == rg.Name);
            if (genre is null)
            {
                genre = new Genre { Name = rg.Name };
                context.Genres.Add(genre);
                await context.SaveChangesAsync();
            }
            genres.Add(genre);
        }
        return genres;
    }

    private async Task<List<Platform>> ResolvePlatformsAsync(List<RawgPlatformWrapperDto>? rawgPlatforms)
    {
        if (rawgPlatforms is null || rawgPlatforms.Count == 0)
            return [];

        var platforms = new List<Platform>();
        foreach (var rp in rawgPlatforms)
        {
            var platformName = rp.Platform.Name;
            var platform = await context.Platforms.FirstOrDefaultAsync(p => p.Name == platformName);
            if (platform is null)
            {
                platform = new Platform { Name = platformName };
                context.Platforms.Add(platform);
                await context.SaveChangesAsync();
            }
            platforms.Add(platform);
        }
        return platforms;
    }

    private static DateOnly ParseReleaseDate(string? released)
    {
        if (DateOnly.TryParse(released, out var date))
            return date;

        return DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
