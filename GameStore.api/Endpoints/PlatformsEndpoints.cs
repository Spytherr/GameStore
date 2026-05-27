namespace GameStore.api;

public static class PlatformsEndpoints
{
    public static void MapPlatformsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/platforms").WithTags("Platforms");

        group.MapGet("/", async (GameStoreContext context) =>
        {
            var platforms = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .ToListAsync(context.Platforms);
            return platforms.Select(p => new { p.Id, p.Name });
        });
    }
}
