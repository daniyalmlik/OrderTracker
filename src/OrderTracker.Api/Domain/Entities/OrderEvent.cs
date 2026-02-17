using OrderTracker.Api.Domain.Enums;

namespace OrderTracker.Api.Domain.Entities;

public sealed class OrderEvent
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public OrderEventType EventType { get; init; }
    public required string EventData { get; init; }
    public required string IdempotencyKey { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public Order Order { get; init; } = null!;
}
