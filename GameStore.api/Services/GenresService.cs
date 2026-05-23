using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class GenresService(GameStoreContext context) : IGenresService
{
    public async Task<List<GenreDto>> GetAllAsync()
    {
        return await context.Genres
            .Select(genre => new GenreDto(genre.Id, genre.Name))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceResult<GenreDto>> GetByIdAsync(int id)
    {
        var genre = await context.Genres.FindAsync(id);

        if (genre is null)
            return ServiceResult<GenreDto>.NotFound($"Genre with ID {id} was not found.");

        return ServiceResult<GenreDto>.Success(new GenreDto(genre.Id, genre.Name));
    }
}
