using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public static class DataExtensions
{
    public static void MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameStoreContext>();
        context.Database.Migrate();
    }

    public static async Task SeedRolesAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = ["Buyer", "Seller"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    public static async Task SeedGamesFromRawgAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GameStoreContext>();

        if (await context.Games.AnyAsync())
            return;

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var apiKey = configuration["Rawg:ApiKey"];
        var baseUrl = configuration["Rawg:BaseUrl"];

        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            Console.WriteLine("[Seed] Skipping RAWG seeding — no API key configured.");
            return;
        }

        Console.WriteLine("[Seed] Seeding games from RAWG API...");

        var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl!) };
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        var allGames = new List<RawgGameSearchResultDto>();

        for (int page = 1; page <= 5; page++)
        {
            var response = await httpClient.GetAsync(
                $"/api/games?key={apiKey}&page_size=20&page={page}&ordering=-added&metacritic=65,100");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Seed] RAWG API returned {response.StatusCode} on page {page}, stopping.");
                break;
            }

            var content = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<RawgSearchResponseDto>(content, jsonOptions);

            if (result?.Results is null || result.Results.Count == 0)
                break;

            allGames.AddRange(result.Results);
        }

        Console.WriteLine($"[Seed] Fetched {allGames.Count} games from RAWG.");

        var genreCache = new Dictionary<string, Genre>();
        var platformCache = new Dictionary<string, Platform>();

        int seededGamesCount = 0;

        foreach (var rawgGame in allGames)
        {
            var isFreeToPlay = rawgGame.Tags?.Any(t => t.Name.Contains("Free to Play", StringComparison.OrdinalIgnoreCase)) == true;
            if (isFreeToPlay)
                continue;

            var titleExists = await context.Games.AnyAsync(g => g.Title == rawgGame.Name);
            if (titleExists)
                continue;

            var gameGenres = new List<Genre>();
            if (rawgGame.Genres is not null)
            {
                foreach (var rg in rawgGame.Genres)
                {
                    if (!genreCache.TryGetValue(rg.Name, out var genre))
                    {
                        genre = await context.Genres.FirstOrDefaultAsync(g => g.Name == rg.Name);
                        if (genre is null)
                        {
                            genre = new Genre { Name = rg.Name };
                            context.Genres.Add(genre);
                            await context.SaveChangesAsync();
                        }
                        genreCache[rg.Name] = genre;
                    }
                    gameGenres.Add(genre);
                }
            }

            var gamePlatforms = new List<Platform>();
            if (rawgGame.Platforms is not null)
            {
                foreach (var rp in rawgGame.Platforms)
                {
                    var platformName = rp.Platform.Name;
                    if (!platformCache.TryGetValue(platformName, out var platform))
                    {
                        platform = await context.Platforms.FirstOrDefaultAsync(p => p.Name == platformName);
                        if (platform is null)
                        {
                            platform = new Platform { Name = platformName };
                            context.Platforms.Add(platform);
                            await context.SaveChangesAsync();
                        }
                        platformCache[platformName] = platform;
                    }
                    gamePlatforms.Add(platform);
                }
            }

            DateOnly releaseDate = DateOnly.TryParse(rawgGame.Released, out var parsed)
                ? parsed
                : DateOnly.FromDateTime(DateTime.UtcNow);

            context.Games.Add(new Game
            {
                Title = rawgGame.Name,
                Genres = gameGenres,
                Platforms = gamePlatforms,
                ImageUrl = rawgGame.BackgroundImage,
                ReleaseDate = releaseDate,
                RawgId = rawgGame.Id,
                Rating = rawgGame.Rating
            });

            seededGamesCount++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[Seed] Seeded {seededGamesCount} games, {genreCache.Count} genres, and {platformCache.Count} platforms.");

        httpClient.Dispose();
    }

    public static void AddGameStoreDatabase(this WebApplicationBuilder builder, string? connectionString)
    {
        builder.Services.AddSqlServer<GameStoreContext>(connectionString);
    }
}
