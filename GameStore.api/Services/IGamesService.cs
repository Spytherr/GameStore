namespace GameStore.api;

public interface IGamesService
{
    Task<List<GameSummaryDto>> GetAllAsync();
    Task<ServiceResult<GameDetailsDto>> GetByIdAsync(int id);
    Task<ServiceResult<GameDetailsDto>> CreateAsync(CreateGameDto dto);
    Task<ServiceResult> UpdateAsync(int id, UpdateGameDto dto);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> ApplyDiscountAsync(int id, decimal discountPercentage);
    Task<ServiceResult> RemoveDiscountAsync(int id);
}
