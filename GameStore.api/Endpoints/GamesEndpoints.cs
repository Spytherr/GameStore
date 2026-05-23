namespace GameStore.api;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";
    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet("/", async (IGamesService service) =>
            await service.GetAllAsync());

        // GET /games/{id}
        group.MapGet("/{id}", async (int id, IGamesService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.ToHttpResult();
        }).WithName(GetGameEndpointName);

        // POST /games — Seller only
        group.MapPost("/", async (CreateGameDto newGame, IGamesService service) =>
        {
            var result = await service.CreateAsync(newGame);
            return result.ToCreatedHttpResult(GetGameEndpointName, game => new { id = game.Id });
        }).RequireAuthorization("SellerOnly");

        // PUT /games/{id} — Seller only
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, IGamesService service) =>
        {
            var result = await service.UpdateAsync(id, updatedGame);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");

        // DELETE /games/{id} — Seller only
        group.MapDelete("/{id}", async (int id, IGamesService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");
    }
}
