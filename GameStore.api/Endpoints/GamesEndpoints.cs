namespace GameStore.api;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGame";

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games").WithTags("Games");

        group.MapGet("/", async ([AsParameters] GamesQueryDto query, IGamesService service) =>
            await service.GetAllAsync(query))
            .AddEndpointFilter<ValidationFilter<GamesQueryDto>>();

        group.MapGet("/{id}", async (int id, IGamesService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.ToHttpResult();
        }).WithName(GetGameEndpointName);

        group.MapPost("/", async (CreateGameDto newGame, IGamesService service) =>
        {
            var result = await service.CreateAsync(newGame);
            return result.ToCreatedHttpResult(GetGameEndpointName, game => new { id = game.Id });
        }).RequireAuthorization("SellerOnly")
          .AddEndpointFilter<ValidationFilter<CreateGameDto>>();

        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, IGamesService service) =>
        {
            var result = await service.UpdateAsync(id, updatedGame);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly")
          .AddEndpointFilter<ValidationFilter<UpdateGameDto>>();

        group.MapDelete("/{id}", async (int id, IGamesService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.ToHttpResult();
        }).RequireAuthorization("SellerOnly");
    }
}
