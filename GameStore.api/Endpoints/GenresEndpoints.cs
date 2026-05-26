namespace GameStore.api;

public static class GenresEndpoints
{
    public static void MapGenresEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/genres").WithTags("Genres");

        // GET /genres
        group.MapGet("/", async (IGenresService service) =>
            await service.GetAllAsync());
    }
}
