namespace GameStore.api;

public interface IGameOffersService
{
    Task<ServiceResult<List<GameOfferDto>>> GetByGameIdAsync(int gameId);
    Task<ServiceResult<GameOfferDto>> CreateAsync(int gameId, CreateGameOfferDto dto, string sellerId);
    Task<ServiceResult> UpdateAsync(int offerId, UpdateGameOfferDto dto, string sellerId);
    Task<ServiceResult> DeleteAsync(int gameId, int offerId, string sellerId);
    Task<ServiceResult> ApplyDiscountAsync(int offerId, decimal discountPercentage, string sellerId);
    Task<ServiceResult> RemoveDiscountAsync(int offerId, string sellerId);
}
