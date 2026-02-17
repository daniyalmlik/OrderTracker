using OrderTracker.Api.Features.Orders.Dtos;

namespace OrderTracker.Api.Features.Orders.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderDto>> ListUserOrdersAsync(int userId, CancellationToken ct = default);
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request, CancellationToken ct = default);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, int userId, UpdateOrderStatusRequest request, CancellationToken ct = default);
    Task CancelOrderAsync(int orderId, int userId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderEventDto>> GetOrderEventsAsync(int orderId, int userId, CancellationToken ct = default);
}
