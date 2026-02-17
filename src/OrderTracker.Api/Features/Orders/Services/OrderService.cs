using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderTracker.Api.Data;
using OrderTracker.Api.Domain.Entities;
using OrderTracker.Api.Domain.Enums;
using OrderTracker.Api.Features.Orders.Dtos;
using OrderTracker.Api.Infrastructure.Kafka;

namespace OrderTracker.Api.Features.Orders.Services;

public sealed class OrderService(AppDbContext db, IKafkaProducerService kafka) : IOrderService
{
    public async Task<IReadOnlyList<OrderDto>> ListUserOrdersAsync(int userId, CancellationToken ct = default)
    {
        var orders = await db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        return orders.Select(ToDto).ToList();
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        var order = new Order
        {
            UserId = userId,
            ItemName = request.ItemName,
            Quantity = request.Quantity,
            Status = OrderStatus.Placed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        var payload = BuildPayload(
            eventType: OrderEventType.OrderPlaced,
            orderId: order.Id,
            userId: userId,
            data: new { itemName = order.ItemName, quantity = order.Quantity },
            idempotencyKey: $"order-{order.Id}-orderplaced-placed");

        await kafka.PublishAsync(payload, ct);

        return ToDto(order);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(
        int orderId, int userId, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        var order = await FindOwnedOrderAsync(orderId, userId, ct);
        var oldStatus = order.Status;

        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var payload = BuildPayload(
            eventType: OrderEventType.StatusChanged,
            orderId: order.Id,
            userId: userId,
            data: new { oldStatus = oldStatus.ToString(), newStatus = request.NewStatus.ToString() },
            idempotencyKey: $"order-{order.Id}-statuschanged-{request.NewStatus.ToString().ToLower()}");

        await kafka.PublishAsync(payload, ct);

        return ToDto(order);
    }

    public async Task CancelOrderAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var order = await FindOwnedOrderAsync(orderId, userId, ct);

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new InvalidOperationException(
                $"Cannot cancel an order with status '{order.Status}'.");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        var payload = BuildPayload(
            eventType: OrderEventType.OrderCancelled,
            orderId: order.Id,
            userId: userId,
            data: new { itemName = order.ItemName },
            idempotencyKey: $"order-{order.Id}-ordercancelled-cancelled");

        await kafka.PublishAsync(payload, ct);
    }

    public async Task<IReadOnlyList<OrderEventDto>> GetOrderEventsAsync(
        int orderId, int userId, CancellationToken ct = default)
    {
        await FindOwnedOrderAsync(orderId, userId, ct);

        var events = await db.OrderEvents
            .Where(e => e.OrderId == orderId)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync(ct);

        return events.Select(ToEventDto).ToList();
    }

    private async Task<Order> FindOwnedOrderAsync(int orderId, int userId, CancellationToken ct)
    {
        var order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);

        return order ?? throw new KeyNotFoundException("Order not found.");
    }

    private static KafkaEventPayload BuildPayload(
        OrderEventType eventType, int orderId, int userId, object data, string idempotencyKey)
    {
        return new KafkaEventPayload(
            EventId: Guid.NewGuid(),
            EventType: eventType.ToString(),
            OrderId: orderId,
            UserId: userId,
            Data: data,
            Timestamp: DateTime.UtcNow,
            IdempotencyKey: idempotencyKey);
    }

    private static OrderDto ToDto(Order o) =>
        new(o.Id, o.UserId, o.ItemName, o.Quantity, o.Status.ToString(), o.CreatedAt, o.UpdatedAt);

    private static OrderEventDto ToEventDto(Domain.Entities.OrderEvent e) =>
        new(e.Id, e.OrderId, e.EventType.ToString(), e.EventData, e.IdempotencyKey, e.CreatedAt);
}
