namespace GameStore.api;

public static class RawgEndpoints
{
    public static void MapRawgEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/rawg")
            .RequireAuthorization("SellerOnly")
            .RequireRateLimiting("write")
            .WithTags("RAWG Import");

        group.MapGet("/search", async (string query, IRawgService service) =>
        {
            var result = await service.SearchAsync(query);
            return result.ToHttpResult();
        });
        group.MapGet("/games/{rawgId}", async (int rawgId, IRawgService service) =>
        {
            var result = await service.GetGameDetailsAsync(rawgId);
            return result.ToHttpResult();
        }).WithDescription("Get detailed information about a game from RAWG by its RAWG ID.");

        group.MapPost("/import", async (RawgImportDto dto, IRawgService service) =>
        {
            var result = await service.ImportAsync(dto.RawgId);
            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<RawgImportDto>>()
          .WithDescription("Import a game from RAWG by its RAWG ID.");
    }
}
