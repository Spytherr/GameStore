using System;
using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public static class GenresEndpoints
{
    public static void MapGenresEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/genres");
        // GET /genres
        group.MapGet("/", async (GameStoreContext context) => await context.Genres.Select(genre => new GenreDto(
            genre.Id,
            genre.Name
        )).AsNoTracking().ToListAsync());
    }

}
