using Microsoft.EntityFrameworkCore;

namespace GameStore.api;

public class OrdersService(
    GameStoreContext context,
    IPaymentService paymentService) : IOrdersService
{
    public async Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto, string buyerId)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            return ServiceResult<OrderDto>.ValidationError("Order must contain at least one item.");

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var orderItems = new List<OrderItem>();

            foreach (var item in dto.Items)
            {
                var offer = await context.GameOffers
                    .Include(o => o.Game)
                    .Include(o => o.Seller)
                    .Include(o => o.Platform)
                    .FirstOrDefaultAsync(o => o.Id == item.GameOfferId);

                if (offer is null)
                    return ServiceResult<OrderDto>.NotFound(
                        $"Game offer with ID {item.GameOfferId} was not found.");

                if (offer.Stock < item.Quantity)
                    return ServiceResult<OrderDto>.ValidationError(
                        $"Insufficient stock for \"{offer.Game!.Title}\". " +
                        $"Requested: {item.Quantity}, available: {offer.Stock}.");

                var unitPrice = offer.IsOnSale
                    ? Math.Round(offer.Price * (1 - offer.DiscountPercentage / 100), 2)
                    : offer.Price;

                offer.Stock -= item.Quantity;

                orderItems.Add(new OrderItem
                {
                    GameOfferId = offer.Id,
                    GameTitle = offer.Game!.Title,
                    SellerName = offer.Seller?.DisplayName ?? "Unknown",
                    PlatformName = offer.Platform?.Name ?? "Unknown",
                    UnitPrice = unitPrice,
                    Quantity = item.Quantity
                });
            }

            var totalAmount = orderItems.Sum(oi => oi.UnitPrice * oi.Quantity);

            var paymentResult = await paymentService.ProcessPaymentAsync(totalAmount, "PLN");
            if (!paymentResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return ServiceResult<OrderDto>.ValidationError(
                    $"Payment failed: {paymentResult.ErrorMessage}");
            }

            var order = new Order
            {
                BuyerId = buyerId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Paid,
                PaymentTransactionId = paymentResult.TransactionId,
                Items = orderItems
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return ServiceResult<OrderDto>.Success(MapToDto(order));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ServiceResult<List<OrderDto>>> GetByBuyerAsync(string buyerId)
    {
        var orders = await context.Orders
            .Where(o => o.BuyerId == buyerId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .AsNoTracking()
            .ToListAsync();

        return ServiceResult<List<OrderDto>>.Success(
            orders.Select(MapToDto).ToList());
    }

    public async Task<ServiceResult<OrderDto>> GetByIdAsync(int orderId, string buyerId)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return ServiceResult<OrderDto>.NotFound(
                $"Order with ID {orderId} was not found.");

        if (order.BuyerId != buyerId)
            return ServiceResult<OrderDto>.Forbidden(
                "You do not have permission to view this order.");

        return ServiceResult<OrderDto>.Success(MapToDto(order));
    }

    public async Task<ServiceResult> CancelAsync(int orderId, string buyerId)
    {
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order is null)
            return ServiceResult.NotFound(
                $"Order with ID {orderId} was not found.");

        if (order.BuyerId != buyerId)
            return ServiceResult.Forbidden(
                "You do not have permission to cancel this order.");

        if (order.Status != OrderStatus.Paid)
            return ServiceResult.ValidationError(
                $"Only paid orders can be cancelled. Current status: {order.Status}.");

        foreach (var item in order.Items)
        {
            var offer = await context.GameOffers.FindAsync(item.GameOfferId);
            if (offer is not null)
                offer.Stock += item.Quantity;
        }

        order.Status = OrderStatus.Cancelled;
        await context.SaveChangesAsync();

        return ServiceResult.Success();
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.OrderDate,
            order.Status.ToString(),
            order.TotalAmount,
            order.PaymentTransactionId,
            order.Items.Select(oi => new OrderItemDto(
                oi.Id,
                oi.GameTitle,
                oi.SellerName,
                oi.PlatformName,
                oi.UnitPrice,
                oi.Quantity,
                oi.UnitPrice * oi.Quantity
            )).ToList()
        );
    }
}
