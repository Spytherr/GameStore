namespace GameStore.api;

public interface IOrdersService
{
    Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto, string buyerId);
    Task<ServiceResult<List<OrderDto>>> GetByBuyerAsync(string buyerId);
    Task<ServiceResult<OrderDto>> GetByIdAsync(int orderId, string buyerId);
    Task<ServiceResult> CancelAsync(int orderId, string buyerId);
}
