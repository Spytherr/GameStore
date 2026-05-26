namespace GameStore.api;

public interface IGamesService
{
    Task<PagedResult<GameSummaryDto>> GetAllAsync(GamesQueryDto query);
    Task<ServiceResult<GameDetailsDto>> GetByIdAsync(int id);
    Task<ServiceResult<GameDetailsDto>> CreateAsync(CreateGameDto dto);
    Task<ServiceResult> UpdateAsync(int id, UpdateGameDto dto);
    Task<ServiceResult> DeleteAsync(int id);
}
