namespace OrderTracker.Api.Infrastructure.Kafka;

public interface IKafkaProducerService
{
    Task PublishAsync(KafkaEventPayload payload, CancellationToken ct = default);
}
