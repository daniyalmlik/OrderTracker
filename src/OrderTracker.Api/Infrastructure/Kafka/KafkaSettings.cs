namespace OrderTracker.Api.Infrastructure.Kafka;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";

    public required string BootstrapServers { get; init; }
    public required string Topic { get; init; }
    public required string ConsumerGroup { get; init; }
}
