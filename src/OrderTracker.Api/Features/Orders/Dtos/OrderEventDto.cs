namespace OrderTracker.Api.Features.Orders.Dtos;

public sealed record OrderEventDto(
    int Id,
    int OrderId,
    string EventType,
    string EventData,
    string IdempotencyKey,
    DateTime CreatedAt);
