namespace OrderTracker.Api.Infrastructure.Kafka;

public sealed record KafkaEventPayload(
    Guid EventId,
    string EventType,
    int OrderId,
    int UserId,
    object Data,
    DateTime Timestamp,
    string IdempotencyKey);
