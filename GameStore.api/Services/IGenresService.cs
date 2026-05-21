namespace GameStore.api;

public interface IGenresService
{
    Task<List<GenreDto>> GetAllAsync();
    Task<ServiceResult<GenreDto>> GetByIdAsync(int id);
}
