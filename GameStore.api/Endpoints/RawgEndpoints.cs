namespace GameStore.api;

public static class RawgEndpoints
{
    public static void MapRawgEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/rawg")
            .RequireAuthorization("SellerOnly")
            .WithTags("RAWG Import");

        group.MapGet("/search", async (string query, IRawgService service) =>
        {
            var result = await service.SearchAsync(query);
            return result.ToHttpResult();
        });

        group.MapPost("/import", async (RawgImportDto dto, IRawgService service) =>
        {
            var result = await service.ImportAsync(dto.RawgId);
            return result.ToHttpResult();
        }).AddEndpointFilter<ValidationFilter<RawgImportDto>>();
    }
}
