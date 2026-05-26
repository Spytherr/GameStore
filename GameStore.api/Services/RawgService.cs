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

        var genre = await ResolveGenreAsync(rawgGame.Genres);

        var titleExists = await context.Games.AnyAsync(g => g.Title == rawgGame.Name);
        if (titleExists)
            return ServiceResult<GameDetailsDto>.Conflict(
                $"A game with the title \"{rawgGame.Name}\" already exists in the catalog.");

        var game = new Game
        {
            Title = rawgGame.Name,
            Description = rawgGame.DescriptionRaw?[..Math.Min(rawgGame.DescriptionRaw.Length, 1000)],
            GenreId = genre.Id,
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
            game.Description,
            game.GenreId,
            genre.Name,
            game.ImageUrl,
            game.ReleaseDate,
            game.Rating,
            []
        ));
    }

    private async Task<Genre> ResolveGenreAsync(List<RawgGenreDto>? rawgGenres)
    {
        var rawgGenreName = rawgGenres?.FirstOrDefault()?.Name ?? "Other";

        var genre = await context.Genres
            .FirstOrDefaultAsync(g => g.Name == rawgGenreName);

        if (genre is not null)
            return genre;

        genre = new Genre { Name = rawgGenreName };
        context.Genres.Add(genre);
        await context.SaveChangesAsync();

        return genre;
    }

    private static DateOnly ParseReleaseDate(string? released)
    {
        if (DateOnly.TryParse(released, out var date))
            return date;

        return DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
