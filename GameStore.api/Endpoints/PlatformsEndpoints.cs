using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public static class PlatformsEndpoints
{
    public static void MapPlatformsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/platforms").WithTags("Platforms");

        group.MapGet("/", async (GameStoreContext context) =>
        {
            var platforms = await context.Platforms
                .Select(p => new PlatformDto(p.Id, p.Name))
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(platforms);
        });
    }
}
