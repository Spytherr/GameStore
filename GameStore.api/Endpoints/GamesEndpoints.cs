using System;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";
    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");
        // GET /games
        group.MapGet("/", async (GameStoreContext context) => await context.Games.Include(game => game.Genre).Select(game => new GameSummaryDto(
            game.Id,
            game.Title,
            game.Genre!.Name,
            game.Price,
            game.ReleaseDate
        )).AsNoTracking().ToListAsync());

        // GET /games/{id}
        group.MapGet("/{id}", async (int id, GameStoreContext context) =>
        {
            var game = await context.Games.Include(game => game.Genre).FirstOrDefaultAsync(g => g.Id == id);
            return game is null ? Results.NotFound() : Results.Ok(new GameDetailsDto(
                game.Id,
                game.Title,
                game.Genre!.Id,
                game.Price,
                game.ReleaseDate
            ));
        }).WithName(GetGameEndpointName);

        // POST /games
        group.MapPost("/",async (CreateGameDto newgame, GameStoreContext context) =>
        {
            Game game = new()
            {
                Title = newgame.Title,
                GenreId = newgame.GenreId,
                Price = newgame.Price,
                ReleaseDate = newgame.ReleaseDate
            };
            context.Games.Add(game);
            await context.SaveChangesAsync();
            GameDetailsDto gameDetails = new(
                game.Id,
                game.Title,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );
            return Results.CreatedAtRoute(GetGameEndpointName, new { id = game.Id }, gameDetails);
        });

        // PUT /games/{id}
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext context) =>
        {
            var game = await context.Games.FindAsync(id);
            if (game is null)
            {
                return Results.NotFound();
            }
            game.Title = updatedGame.Title;
            game.GenreId = updatedGame.GenreId;
            game.Price = updatedGame.Price;
            game.ReleaseDate = updatedGame.ReleaseDate;
            await context.SaveChangesAsync();
            return Results.NoContent();
        });

        // DELETE /games/{id}
        group.MapDelete("/{id}", async (int id, GameStoreContext context) =>
        {
            await context.Games.Where(g => g.Id == id).ExecuteDeleteAsync();
            return Results.NoContent();
        });
    }
}
